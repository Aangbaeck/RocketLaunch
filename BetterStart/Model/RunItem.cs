using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Serilog;
using Exception = System.Exception;

namespace BetterStart.Model
{
    [ProtoContract]
    public class RunItem
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Group { get; set; }
        [ProtoMember(3)]
        public ItemType Type { get; set; }
        [ProtoMember(4)]
        public string URI { get; set; }
        [ProtoMember(5)] 
        public int RunNrOfTimes { get; set; } = 0;
        public string FileName
        {
            get
            {
                try
                {
                    if(Type == ItemType.File)
                        return System.IO.Path.GetFileName(URI);
                }
                catch (Exception e)
                {
                    Log.Error(e,$"Could not convert URI {URI} to filename");
                    
                }
                return URI;
            }
        }
        public string FileNameWithoutExtension
        {
            get
            {
                try
                {
                    if (Type == ItemType.File)
                        return System.IO.Path.GetFileNameWithoutExtension(URI);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Could not convert URI {URI} to filename without extension");

                }
                return URI;
            }
        }
    }

    public enum ItemType
    {
        File,
        Setting,
        Directory

    }
    public static class CommonControlPanel
    {
        public static List<RunItem> Settings { get; } = new List<RunItem>()
        {
            new RunItem() { Name="Settings home page", Group="Home", URI="ms-settings:", Type=ItemType.Setting }
        };
        //TODO add more https://winaero.com/blog/ms-settings-commands-windows-10/


    }


}
