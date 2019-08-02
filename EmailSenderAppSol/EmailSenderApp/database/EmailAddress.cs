using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace EmailSenderApp.database {
  public class EmailAddress {
    public int Id {get;set;}
    public string Name {get;set;}
    public string Url {get;set;}
    public string GroupName {get;set;}
    public int GroupId {get;set;}
    
    public Dictionary<string,object> AddressDict() {
      return new Dictionary<string,object>() {
        {"Id",Id},
        {"Name",Name},
        {"Url",Url},
        {"GroupName",GroupName},
        {"GroupId",GroupId}
      };
    }
  }
}