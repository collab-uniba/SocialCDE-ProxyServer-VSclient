using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.TeamFoundationServer;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.CodePlex
{
    class CodePlexService : TeamFoundationServerService, IService
    {
        private String web_magic_user = "MCLWEB";

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        internal CodePlexService()
            : base()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Username on the service.</param>
        /// <param name="password">Password on the service.</param>
        /// <param name="host">Root path of the service.</paparam>
        internal CodePlexService(String username, String password, String host)
            : base(username, password, "snd", host)
        { }

        #endregion

        #region IService

        int IService.Version
        {
            get { return 1; }
        }

        string IService.Name
        {
            get { return "CodePlex"; }
        }

        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.TFSTeamProject);
            features.Add(FeaturesType.IterationNetwork);
            features.Add(FeaturesType.InteractiveNetwork);

            return features;
        }

        List<FeaturesType> IService.GetPrivateFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.TFSAuthenticationWithoutDomain);

            return features;
        }

        List<FeaturesType> IService.GetScoredFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.TFSTeamProject);

            return features;
        }

        object IService.Get(FeaturesType feature, params object[] param)
        {
            object result = null;

            switch (feature)
            {
                case FeaturesType.IterationNetwork:
                    result = GetDynamicFriends();
                    break;
                case FeaturesType.InteractiveNetwork:
                    result = GetInteractiveFriends();
                    break;
                case FeaturesType.TFSCollection:
                    result = GetPeopleInAllCollections();
                    break;
                case FeaturesType.TFSTeamProject:
                    result = GetPeopleInAllProjects();
                    break;
                default:
                    throw new NotImplementedException("Use GetAvailableFeatures() to know the implemented methods");
            }

            return result;
        }

        #endregion

        #region Private

        new string[] GetDynamicFriends()
        {
            List<string> dynamicFriend = new List<string>();

            foreach (TfsTeamProjectCollection collection in GetCollections())
            {
                WorkItemStore wis = collection.GetService<WorkItemStore>();

                WorkItemCollection allWi = wis.Query("SELECT * FROM WorkItems WHERE [State] = 'Active' OR [State] = 'Fixed'");
                foreach (WorkItem item in allWi)
                {
                    List<string> peopleInWorkItem = new List<string>();
                    try{
                        peopleInWorkItem.Add(item.Fields[CoreField.AssignedTo].Value.ToString());
                            }catch
                            {
                                System.Diagnostics.Debug.WriteLine("prova " + item);
                                System.Diagnostics.Debug.WriteLine(item.Title);
                                System.Diagnostics.Debug.WriteLine(item.Fields[CoreField.AssignedTo]);
                                System.Diagnostics.Debug.WriteLine(item.Fields[CoreField.AssignedTo].Value);
                            }

                    //peopleInWorkItem.Add(item.Fields[CoreField.AssignedTo].Value.ToString());
                    foreach (Revision revision in item.Revisions)
                    {
                        peopleInWorkItem.Add(revision.Fields[CoreField.AssignedTo].Value.ToString());
                        if (revision.Fields[CoreField.ChangedBy].Value.ToString() == web_magic_user)
                        {
                            if (revision.Fields.Contains("CodePlex Updated By"))
                                peopleInWorkItem.Add(revision.Fields["CodePlex Updated By"].Value + "_cp");
                        }
                        else
                        {
                            peopleInWorkItem.Add(revision.Fields[CoreField.ChangedBy].Value.ToString());
                        }
                    }
                    if (peopleInWorkItem.Contains(_username))
                        dynamicFriend.AddRange(peopleInWorkItem);
                }

            }

            return dynamicFriend.Distinct().ToArray();
        }

        new SCollection[] GetInteractiveFriends()
        {
            List<SCollection> result = new List<SCollection>();
            foreach (TfsTeamProjectCollection collection in GetCollections())
            {
                List<SWorkItem> sworkitems = new List<SWorkItem>();
                WorkItemStore workitemStore = collection.GetService<WorkItemStore>();
                WorkItemCollection workitemCollection = workitemStore.Query("SELECT * FROM WorkItems");
                foreach (WorkItem wi in workitemCollection)
                {
                    List<string> people = new List<string>();
                    people.Add(wi.Fields[CoreField.AssignedTo].Value.ToString());
                    foreach (Revision revision in wi.Revisions)
                    {
                        people.Add(revision.Fields[CoreField.AssignedTo].Value.ToString());
                        if (revision.Fields[CoreField.ChangedBy].Value.ToString() == web_magic_user)
                        {
                            if (revision.Fields.Contains("CodePlex Updated By"))
                                people.Add(revision.Fields["CodePlex Updated By"].Value + "_cp");
                        }
                        else
                        {
                            people.Add(revision.Fields[CoreField.ChangedBy].Value.ToString());
                        }
                    }
                    sworkitems.Add(new SWorkItem() { Name = wi.Id.ToString(), InvolvedUsers = people.Distinct().ToArray() });
                }

                List<SFile> sfiles = new List<SFile>();
                VersionControlServer versioncontrolServer = collection.GetService<VersionControlServer>();
                ItemSet itemset = versioncontrolServer.GetItems(@"$\", VersionSpec.Latest, RecursionType.Full, DeletedState.Any, ItemType.File);
                foreach (Item file in itemset.Items)
                {
                    List<string> people = new List<string>();
                    IEnumerable<Changeset> changesets = versioncontrolServer.QueryHistory(file.ServerItem, VersionSpec.Latest, 0, RecursionType.Full, null, null, null, Int32.MaxValue, true, false).Cast<Changeset>();
                    foreach (Changeset cs in changesets)
                        people.Add(cs.Committer);
                    sfiles.Add(new SFile() { Name = file.ServerItem.ToString(), InvolvedUsers = people.Distinct().ToArray() });
                }
                result.Add(new SCollection() { Uri = collection.Uri.ToString(), WorkItems = sworkitems.ToArray(), Files = sfiles.ToArray() });
            }

            return result.ToArray();
        }

        #endregion
    }
}
