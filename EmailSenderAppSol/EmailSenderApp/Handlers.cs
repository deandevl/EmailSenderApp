using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using BasicWebServerLib;
using BasicWebServerLib.Events;
using BasicWebServerLib.HttpCommon;
using EmailSenderApp.database;
using LiteDB;

namespace EmailSenderApp {
  public class Handlers {
    private readonly string _serverBaseFolder;
    private readonly JavaScriptSerializer _serializer;
    private readonly Helpers _helpers;
    private readonly Dictionary<string, Action> _actions;
    private Dictionary<string, object> _requestDictionary;
    private HttpConnectionDetails _httpDetails;
    private LiteCollection<EmailGroup> _groupCollection;
    private LiteCollection<EmailAddress> _addressCollection;
    
    public Handlers(string serverBaseFolder, Dictionary<string,string> emailDict) {
      _serverBaseFolder = serverBaseFolder;
      _serializer = new JavaScriptSerializer();
      _helpers = new Helpers();

      
      _actions = new Dictionary<string, Action>() {
        {"getDbFiles", () => {
          try {
            ArrayList dbFilePaths = new ArrayList();
            ArrayList dbFileNames = new ArrayList();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            Invoker invoker = new Invoker(fbd);
            if(DialogResult.OK == invoker.Invoke()) {
              string[] files = Directory.GetFiles(fbd.SelectedPath);
              foreach(string filepath in files) {
                FileInfo fileInfo = new FileInfo(filepath);
                if(fileInfo.Extension.Equals(".db")) {
                  dbFilePaths.Add(filepath);
                  dbFileNames.Add(fileInfo.Name);
                }
              }
            }
            Dictionary<string, object> responseDict = new Dictionary<string, object>() {
              {"dbfolder", fbd.SelectedPath},
              {"dbfilepaths", dbFilePaths},
              {"dbfilenames", dbFileNames}
            };
            string responseStr = _serializer.Serialize(responseDict);
            _helpers.SendHttpTextResponse(_httpDetails.Response,responseStr);
          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }
        }},
        {"selectdb", () => {
          try {
            string dbpath = (string)_requestDictionary["dbpath"];
            LiteDatabase db = new LiteDatabase(dbpath);
            _groupCollection = db.GetCollection<EmailGroup>("GroupCollection");
            _addressCollection = db.GetCollection<EmailAddress>("AddressCollection");
            ArrayList groupNames = GetGroupNames();
            string responseStr = _serializer.Serialize(groupNames);
            _helpers.SendHttpTextResponse(_httpDetails.Response, responseStr);
          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }  
        }}, 
        {"getAddresses", () => {
          try {
            string groupName = (string)_requestDictionary["groupname"];
            IEnumerable<EmailAddress> addresses = _addressCollection.Find(x => x.GroupName.Equals(groupName));
            ArrayList programsList = new ArrayList();
            foreach(EmailAddress address in addresses) {
              programsList.Add(address.AddressDict());
            }
            string responseStr = _serializer.Serialize(programsList);
            _helpers.SendHttpTextResponse(_httpDetails.Response,responseStr);
          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }
        }},
        {"sendEmails", () => {
          try {
            string groupName = (string)_requestDictionary["groupname"];
            string subject = (string)_requestDictionary["subject"];
            if(subject == null) {
              subject = "No Subject";
            }
            string messageFilePath = (string)_requestDictionary["messagepath"];
            string imageFilePath = (string)_requestDictionary["imagepath"];
            
            string fromName = emailDict["EmailFromName"];
            string fromAddress = emailDict["EmailFromAddress"];
            string contentId = emailDict["EmailImageContentID"];
            int emailPort = Int32.Parse(emailDict["EmailPort"]);
            string emailUser = emailDict["EmailUser"];
            string emailPass = emailDict["EmailPass"];
            
            string messageBody = File.ReadAllText(messageFilePath);
            
            SmtpClient smtpServer = new SmtpClient(emailDict["EmailServer"]);
            MailMessage mail = new MailMessage {
              From = new MailAddress(fromAddress, fromName),
              Subject = subject
            };
            
            IEnumerable<EmailAddress> addresses = _addressCollection.Find(x => x.GroupName.Equals(groupName));
            foreach(EmailAddress address in addresses) {
              mail.Bcc.Add(address.Url);
            }
            
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(messageBody, null, MediaTypeNames.Text.Html);
            if(imageFilePath != null) {
              LinkedResource linkedResource = new LinkedResource(imageFilePath, MediaTypeNames.Image.Jpeg) {
                ContentId = contentId,
              };
              alternateView.LinkedResources.Add(linkedResource);
            }
            mail.AlternateViews.Add(alternateView);
            
            smtpServer.Port = emailPort;
            smtpServer.Credentials = new NetworkCredential(emailUser,emailPass);
            smtpServer.EnableSsl = true;
            
            smtpServer.Send(mail);
            _helpers.SendHttpTextResponse(_httpDetails.Response, "Successfully sent emails for group " + groupName);
          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }  
        }},
        {"checkAddresses", () => {
          try {
            ArrayList badList = new ArrayList();
            string groupName = (string)_requestDictionary["groupname"];
            IEnumerable<EmailAddress> addresses = _addressCollection.Find(x => x.GroupName.Equals(groupName));
            foreach(EmailAddress address in addresses) {
              if(!checkAddress(address.Url) || !checkHost(address.Url)) {
                badList.Add(address.AddressDict());
              }
            }
            string responseStr = _serializer.Serialize(badList);
            _helpers.SendHttpTextResponse(_httpDetails.Response,responseStr);
          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }
        }},
        {"addAddress", () => {
          try {
            Dictionary<string, object> addressDict =
              (Dictionary<string, object>)_requestDictionary["address"];
            EmailAddress newAddress = AddAddress(addressDict);
            if(newAddress != null) {
              //return a list of group names and the new address
              Dictionary<string, object> responseDict = new Dictionary<string, object>() {
                {"group_names", GetGroupNames()},
                {"backup_address", newAddress.AddressDict()}
              };
              string responseStr = _serializer.Serialize(responseDict);
              _helpers.SendHttpTextResponse(_httpDetails.Response, responseStr);
            } else {
              _helpers.SendHttpResponse(400, "Duplicate Address Found",new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
            }
          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }
        }}, 
        {"updateAddress", () => {
          try {
            Dictionary<string, object> addressDict =
              (Dictionary<string, object>)_requestDictionary["address"];

            EmailAddress backupAddress = UpdateAddress(addressDict);
            if(backupAddress != null) {
              //return a list of group names and the new address
              Dictionary<string, object> responseDict = new Dictionary<string, object>() {
                {"group_names", GetGroupNames()},
                {"backup_address", backupAddress.AddressDict()}
              };
              string responseStr = _serializer.Serialize(responseDict);
              _helpers.SendHttpTextResponse(_httpDetails.Response, responseStr);
            } else {
              _helpers.SendHttpResponse(400, "Address was not located for update", new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
            }
          } catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }   
        }},
        {"deleteAddress", () => {
          try {
            Dictionary<string, object> addressDict = (Dictionary<string, object>)_requestDictionary["address"];
            string groupName = (string)addressDict["GroupName"];  //debug
            string emailName = (string)addressDict["Name"];  //debug
            EmailAddress currentEmailAddress = _addressCollection.FindOne(x => x.GroupName == groupName && x.Name == emailName);
            if(currentEmailAddress != null) {
              int result = DeleteAddress(currentEmailAddress.Id, currentEmailAddress.GroupId);
              //return the current set of group names and backup program
              ArrayList groupNames = GetGroupNames();
              Dictionary<string, object> responseDict = new Dictionary<string, object>() {
                {"backup_address", currentEmailAddress.AddressDict()},
                {"group_names", groupNames}
              };

              string responseStr = _serializer.Serialize(responseDict);
              _helpers.SendHttpTextResponse(_httpDetails.Response, responseStr);
            } else {
              _helpers.SendHttpResponse(400, "Could not locate address",new byte[0],"text/html","MoneyTracker Server", _httpDetails.Response);
            }
          }catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }
        }},
        {"getMessageFilePath", () => {
          try {
            OpenFileDialog ofd = new OpenFileDialog();
            Invoker invoker = new Invoker(ofd);
            if(DialogResult.OK == invoker.Invoke()) {
              string responseStr = ofd.FileName;
              _helpers.SendHttpTextResponse(_httpDetails.Response, responseStr);
            }
          }catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }
        }},
        {"getImageFilePath", () => {
          try {
            OpenFileDialog ofd = new OpenFileDialog();
            Invoker invoker = new Invoker(ofd);
            if(DialogResult.OK == invoker.Invoke()) {
              string responseStr = ofd.FileName;
              _helpers.SendHttpTextResponse(_httpDetails.Response, responseStr);
            }
          }catch(Exception ex) {
            _helpers.SendHttpResponse(500, ex.Message,new byte[0],"text/html","EmailSender Server", _httpDetails.Response);
          }
        }}
      };
    }
    
    public void StartServer() {
      BasicWebServer basicServer = new BasicWebServer(baseFolderPath: _serverBaseFolder, tcpPort:null, httpPrefix:"http://localhost:8082/");
      basicServer.HttpRequestChanged += HttpRequestChanged;
      
      basicServer.Start();
    }

    public void HttpRequestChanged(object sender, EventArgs args) {
      HttpRequestEventArgs httpArgs = (HttpRequestEventArgs)args;
      _httpDetails = httpArgs.Details;
      string body = (string)httpArgs.Body;
      _requestDictionary = _serializer.Deserialize<Dictionary<string, object>>(body);
      
      if(_httpDetails.HttpPath == "emailsender") {
        _actions[(string)_requestDictionary["action"]]();
      }
    }
    
    private ArrayList GetGroupNames() {
      IEnumerable<EmailGroup> groupsList = _groupCollection.FindAll();
      ArrayList groupNames = new ArrayList();
      foreach(EmailGroup group in groupsList) {
        groupNames.Add(group.Name);
      }
      return groupNames;
    }
    private EmailAddress AddAddress(Dictionary<string, object> addressDict) {
      string emailGroupName = (string)addressDict["GroupName"];
      string emailName = (string)addressDict["Name"];
      
      //does the group exist in db?
      int groupId;
      EmailGroup group = _groupCollection.FindOne(x => x.Name.Equals(emailGroupName));
      if(group == null) { //if not then create it
        groupId = AddGroup(emailGroupName);
      } else {
        groupId = group.Id;
      }
      //does the address exist with this group
      EmailAddress emailAddress = _addressCollection.FindOne(x => x.GroupId == groupId && x.Name.Equals(emailName));
      if(emailAddress == null) {
        EmailAddress newAddress = new EmailAddress();
        newAddress.Name = emailName;
        newAddress.Url = (string)addressDict["Url"];
        newAddress.GroupName = emailGroupName;
        newAddress.GroupId = groupId;

        int id = _addressCollection.Insert(newAddress);
        return newAddress;
      } else {
        return null;
      }
    }
    private int DeleteAddress(int addressId,int groupId) {
      int deletedCount = _addressCollection.Delete(x => x.Id == addressId);
      //is this the last address for addresses' group?
      IEnumerable<EmailAddress> addresses = _addressCollection.Find(x => x.GroupId.Equals(groupId));
      if(addresses.Count() == 0) {
        //there are no programs with groupId, so delete the group
        int removed = _groupCollection.Delete(x => x.Id.Equals(groupId));
      }
      return deletedCount;
    }
    private EmailAddress UpdateAddress(Dictionary<string, object> addressDict) {
      string currentGroupName = (string)addressDict["currentGroupName"];
      string updateGroupName = (string)addressDict["updateGroupName"];
      string currentAddressName = (string)addressDict["currentName"];
      string updateAddressName = (string)addressDict["updateName"];
      EmailAddress currentAddress = _addressCollection.FindOne(x => x.GroupName == currentGroupName && x.Name == currentAddressName);
      if(currentAddress != null) {
        //are we changing the group name
        if(!updateGroupName.Equals(currentAddress.GroupName)) {
          //we are changing the group name; so is the current address the last address in the current group?
          //if so then delete the old group
          IEnumerable<EmailAddress> addresses = _addressCollection.Find(x => x.GroupName.Equals(currentAddress.GroupName));
          if(addresses.Count() == 1) {
            //this is  the last program with a GroupName that is changing, so delete the group
            int removed = _groupCollection.Delete(x => x.Name.Equals(currentAddress.GroupName));
          }
        }
        //does the group exist in db?
        int groupId;
        EmailGroup group = _groupCollection.FindOne(x => x.Name.Equals(updateGroupName));
        if(group == null) { //if not then create it
          groupId = AddGroup(updateGroupName);
        } else {
          groupId = group.Id;
        }
        
        EmailAddress emailAddress = new EmailAddress();
        emailAddress.Id = currentAddress.Id;
        emailAddress.Name = updateAddressName;
        emailAddress.GroupName = updateGroupName;
        emailAddress.GroupId = groupId;
        emailAddress.Url = (string)addressDict["Url"];

        bool found = _addressCollection.Update(emailAddress);
        return currentAddress;
      } else {
        return null;
      }
    }
    private int AddGroup(string groupName) {
      EmailGroup emailGroup = new EmailGroup();
      emailGroup.Name = groupName;
      return _groupCollection.Insert(emailGroup);
    }
    private bool checkAddress(string address) {
      string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                        @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + 
                        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
      Regex re = new Regex(strRegex);
      if (re.IsMatch(address))
        return true;
      else
        return false;
    }
    private bool checkHost(string emailAddress) {
      string[] parts = (emailAddress.Split('@'));
      string hostname = parts[1];

      try {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(hostname);
        foreach(IPAddress address in ipHostEntry.AddressList) {
          IPEndPoint endPt = new IPEndPoint(address, 25);
          Socket tempSocket = new Socket(endPt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
          tempSocket.Connect(endPt);
          if(tempSocket.Connected) {
            tempSocket.Close();
            return true;
          }
        }
        return false;
      } catch(Exception ex) {
        Console.WriteLine(ex.Message);
        return false;
      }
    }
  }

  public class Invoker {
    private CommonDialog _invokeDialog;
    private Thread _invokeThread;
    private DialogResult _invokeResult;

    public Invoker(CommonDialog dialog) {
      _invokeDialog = dialog;
      _invokeThread = new Thread(new ThreadStart(InvokeMethod));
      _invokeThread.SetApartmentState(ApartmentState.STA);
      _invokeResult = DialogResult.None;
    }

    public DialogResult Invoke() {
      _invokeThread.Start();
      _invokeThread.Join();
      return _invokeResult;
    }

    private void InvokeMethod() {
      _invokeResult = _invokeDialog.ShowDialog();
    }
  }
}