using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace EmailSenderApp {
  public class Settings {
    public string EmailFromAddress { get; set; }
    public string EmailFromName {get;set;}
    public string EmailImageContentID {get;set;}
    public string EmailServer {get; set;}
    public string EmailPort {get; set;}
    public string EmailUser {get;set;}
    public string EmailPass {get;set;}
  }
  
  public class Program {
    public static void Main(string[] args) {
      string currentDir = Directory.GetCurrentDirectory();
      Dictionary<string,string> emailDict = new Dictionary<string, string>(); 
      
      Settings settings = new Settings();
      string settingsPath = Path.Combine(currentDir, "settings.xml");
      string serverBaseFolder = Path.Combine(currentDir, "html", "dist");

      try {
        if(File.Exists(settingsPath)) {
          XmlSerializer x = new XmlSerializer(typeof(Settings));
          StreamReader reader = new StreamReader(settingsPath);
          settings = (Settings)x.Deserialize(reader);
          emailDict["EmailFromAddress"] = settings.EmailFromAddress;
          emailDict["EmailFromName"] = settings.EmailFromName;
          emailDict["EmailImageContentID"] = settings.EmailImageContentID;
          emailDict["EmailServer"] = settings.EmailServer;
          emailDict["EmailPort"] = settings.EmailPort;
          emailDict["EmailUser"] = settings.EmailUser;
          emailDict["EmailPass"] = settings.EmailPass;
        } else {
          throw new Exception("EmailSender: Missing Settings File");
        }
        Handlers handlers = new Handlers(serverBaseFolder,emailDict);
        handlers.StartServer();
      } catch(Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }
  }
}