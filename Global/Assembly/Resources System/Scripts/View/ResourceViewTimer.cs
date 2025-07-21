using System;

namespace MadApper
{
    public class ResourceViewTimer : ResourcesViewTypeBase
    {      
        public override string GetText(ResourceData data)
        {
            var seconds = (data.UnlimitedTill - DateTime.Now).TotalSeconds;
            var duration = TimeSpan.FromSeconds(seconds);
                        
            return $"{(int)duration.TotalHours}:{duration:mm}:{duration:ss}";         

        }
    }
}
