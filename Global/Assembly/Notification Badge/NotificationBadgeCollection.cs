using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MadApper.Essentials
{
    public abstract class NotificationBadgeCollection : NotificationBadge
    {              
        protected abstract List<NotificationBadge> RetrieveBadges();

        public override bool ShouldShow()
        {
            var badges = RetrieveBadges();
            if (badges == null || !badges.Any()) return false;
            var res = badges.Any(x => x.WillBeShown());
            return res;
        }
    }
}
