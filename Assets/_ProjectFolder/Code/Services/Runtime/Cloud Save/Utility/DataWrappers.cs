using System.Collections.Generic;

namespace Unity.Services.CloudSave
{
    using Models;
    
    public abstract class ItemsWrapper : Dictionary<string, Item> { }
    public class Payload : Dictionary<string, object> { }
}