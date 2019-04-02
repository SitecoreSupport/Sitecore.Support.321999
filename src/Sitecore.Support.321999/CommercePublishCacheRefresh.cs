// Sitecore.Commerce.Connect.CommerceServer.Events.CommercePublishCacheRefresh
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Catalog;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Publishing;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Sitecore.Jobs;

namespace Sitecore.Support.Commerce.Connect.CommerceServer.Events
{
    public class CommercePublishCacheRefresh
    {
    #region fix
    public Job CurrentJob { get; set; }
        public virtual void ClearCache(object sender, EventArgs args)
        {
            var jobName = "Clear commerce caches after publishing, fix for bug #321999";
            var jobCategory = "Commerce publish";
            var siteName = Sitecore.Context.Site.Name;
            var objectWithMethodToRun = this;
            var methodName = "DoClearCache";
            var methodParameters = new Object[] { args };
            var options = new JobOptions(jobName, jobCategory, siteName,
                                                              objectWithMethodToRun, methodName,
                                                              methodParameters);
            this.CurrentJob = JobManager.Start(options);
        }

        public virtual void DoClearCache(EventArgs args)
        {
            CatalogUtility.RefreshTemplateHierarchyCache(true);
            CommerceUtility.RemoveItemFromSitecoreCaches(CommerceConstants.KnownItemIds.CatalogsItem, null);
            PublishOptions publishOptions = GetPublishOptions(args);
            if (publishOptions != null)
            {
                Item val = publishOptions.TargetDatabase.GetItem("/sitecore/content");
                List<Item> list = new List<Item>();
                if (val != null)
                {
                    CheckForNavChildren(publishOptions.RootItem, list);
                    foreach (Item item in list)
                    {
                        CommerceUtility.RemoveItemFromSitecoreCaches(item.ID, null);
                    }
                }
            }
        }
    #endregion fix
    private void CheckForNavChildren(Item item, ICollection<Item> list)
        {
            if (item != null)
            {
                if (CatalogUtility.IsItemDerivedFromCommerceTemplate(item, CommerceConstants.KnownTemplateIds.CommerceNavigationItemTemplate))
                {
                    list.Add(item);
                }
                else
                {
                   foreach (Item child in item.GetChildren())
                    {
                        Item item2 = child;
                        CheckForNavChildren(item2, list);
                    }
                }
            }
        }

        private PublishOptions GetPublishOptions(EventArgs args)
        {
            SitecoreEventArgs val = args as SitecoreEventArgs;
            if (val != null)
            {
                object[] parameters = val.Parameters;
                foreach (object obj in parameters)
                {
                    Publisher val2 = obj as Publisher;
                    if (val2 != null)
                    {
                        return val2.Options;
                    }
                }
            }
            return null;
        }
    }
}