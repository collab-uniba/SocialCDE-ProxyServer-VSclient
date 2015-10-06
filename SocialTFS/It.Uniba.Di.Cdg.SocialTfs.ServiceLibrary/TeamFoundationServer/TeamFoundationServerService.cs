using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.TeamFoundationServer
{
    class TeamFoundationServerService : IService
    {
        #region Attributes

        protected string _username;
        protected TfsConfigurationServer _configurationServer;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        internal TeamFoundationServerService()
        {
            _username = null;
            _configurationServer = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Username on the service.</param>
        /// <param name="password">Password on the service.</param>
        /// <param name="domain">Domain on the service.</param>
        /// <param name="host">Root path of the service.</paparam>
        internal TeamFoundationServerService(String username, String password, String domain, String host)
        {
            _username = username;
            _configurationServer = GetConfigurationServer(username, password, domain, host);
            try
            {
                _configurationServer.Authenticate();
            }
            catch (Exception)
            {
                try
                {
                    Uri url = new Uri(host);
                    String ip = Dns.GetHostAddresses(url.DnsSafeHost).First().ToString();
                    String newHost = url.Scheme + "://" + ip + ":" + url.Port + url.PathAndQuery;
                    _configurationServer = GetConfigurationServer(username, password, domain, newHost);
                    _configurationServer.Authenticate();
                }
                catch (TeamFoundationServiceUnavailableException)
                {
                    //if authentication fail
                    return;   
                }
            }
        }

        #endregion

        #region IService

        int IService.Version
        {
            get { return 1; }
        }

        string IService.Name
        {
            get { return "TeamFoundationServer"; }
        }

        IUser IService.VerifyCredential()
        {
            IUser user;

            try
            {
                TeamFoundationIdentity teamFoundationIdentity;
                _configurationServer.GetAuthenticatedIdentity(out teamFoundationIdentity);
                user = new TeamFoundationServerUser(teamFoundationIdentity.Descriptor.Identifier, teamFoundationIdentity.DisplayName, teamFoundationIdentity.GetAttribute("Domain", String.Empty));
            }
            catch (TeamFoundationServerUnauthorizedException)
            {
                user = null;
            }

            return user;
        }

        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.TFSCollection);
            features.Add(FeaturesType.TFSTeamProject);
            features.Add(FeaturesType.IterationNetwork);
            features.Add(FeaturesType.InteractiveNetwork);

            return features;
        }

        List<FeaturesType> IService.GetPrivateFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.MoreInstance);
            features.Add(FeaturesType.TFSAuthenticationWithDomain);

            return features;
        }

        List<FeaturesType> IService.GetScoredFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.TFSCollection);
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

        protected List<String> GetPeopleInAProject(TeamProject teamProject)
        {
            TfsTeamProjectCollection collection = teamProject.TeamProjectCollection;
            List<String> people = new List<string>();

            var gss = collection.GetService<IGroupSecurityService>();

            Identity SIDS = gss.ReadIdentity(SearchFactor.EveryoneApplicationGroup, null, QueryMembership.Direct);
            Identity[] GroupIds = gss.ReadIdentities(SearchFactor.Sid, SIDS.Members, QueryMembership.None);

            var Groups = GroupIds.Where(u => u.Domain == teamProject.ArtifactUri.ToString()).ToArray();

            foreach (var Group in Groups)
            {
                Identity SubSIDS = gss.ReadIdentity(SearchFactor.Sid,
                Group.Sid,
                QueryMembership.Expanded);

                if (SubSIDS.Members.Length == 0)
                {
                    continue;
                }

                Identity[] MemberIds = gss.ReadIdentities(SearchFactor.Sid, SubSIDS.Members, QueryMembership.None);

                var Members = MemberIds.Where(u => !u.SecurityGroup).ToArray();
                foreach (var member in Members)
                {
                    people.Add(member.Sid);
                }

            }

            return people.Distinct().ToList();
        }

        protected List<String> GetPeopleInACollection(TfsTeamProjectCollection collection)
        {
            List<String> people = new List<string>();

            var gss = collection.GetService<IGroupSecurityService>();

            Identity SIDS = gss.ReadIdentity(SearchFactor.EveryoneApplicationGroup,
              null,
              QueryMembership.Direct);
            Identity[] GroupIds = gss.ReadIdentities(SearchFactor.Sid, SIDS.Members, QueryMembership.None);

            foreach (var Group in GroupIds)
            {
                Identity SubSIDS = gss.ReadIdentity(SearchFactor.Sid, Group.Sid, QueryMembership.Expanded);

                if (SubSIDS.Members.Length == 0)
                {
                    continue;
                }

                Identity[] MemberIds = gss.ReadIdentities(SearchFactor.Sid, SubSIDS.Members, QueryMembership.None);

                var Members = MemberIds.Where(u => !u.SecurityGroup).ToArray();
                foreach (var member in Members)
                {
                    people.Add(member.Sid);
                }
            }

            return people.Distinct().ToList();
        }

        protected List<TeamProject> GetProjects(TfsTeamProjectCollection collection)
        {
            VersionControlServer vcs = collection.GetService<VersionControlServer>();

            return new List<TeamProject>(vcs.GetAllTeamProjects(false));
        }

        protected List<TfsTeamProjectCollection> GetCollections()
        {
            CatalogNode configurationServerNode = _configurationServer.CatalogNode;
            List<TfsTeamProjectCollection> collections = new List<TfsTeamProjectCollection>();

            // Query the children of the configuration server node for all of the team project collection nodes
            ReadOnlyCollection<CatalogNode> tpcNodes = configurationServerNode.QueryChildren(
                    new Guid[] { CatalogResourceTypes.ProjectCollection },
                    false,
                    CatalogQueryOptions.None);

            foreach (CatalogNode tpcNode in tpcNodes)
            {
                Guid tpcId = new Guid(tpcNode.Resource.Properties["InstanceId"]);
                TfsTeamProjectCollection tpc = _configurationServer.GetTeamProjectCollection(tpcId);

                collections.Add(tpc);
            }

            return collections;
        }

        protected String[] GetPeopleInAllCollections()
        {
            List<String> people = new List<string>();

            List<TfsTeamProjectCollection> collections = GetCollections();

            foreach (TfsTeamProjectCollection collection in collections)
                people.AddRange(GetPeopleInACollection(collection));

            return people.ToArray();
        }

        protected String[] GetPeopleInAllProjects()
        {
            List<String> people = new List<string>();

            List<TfsTeamProjectCollection> collections = GetCollections();

            foreach (TfsTeamProjectCollection collection in collections)
                foreach (TeamProject teamProject in GetProjects(collection))
                    people.AddRange(GetPeopleInAProject(teamProject));

            return people.ToArray();
        }

        protected TfsConfigurationServer GetConfigurationServer(String username, String password, String domain, String host)
        {
            ICredentials credential = new NetworkCredential(username, password, domain);
            Uri uri = new Uri(host);
            return new TfsConfigurationServer(uri, credential);
        }

        protected string[] GetDynamicFriends()
        {
            List<string> dynamicFriend = new List<string>();

            foreach (TfsTeamProjectCollection collection in GetCollections())
            {
                WorkItemStore wis = collection.GetService<WorkItemStore>();

                WorkItemCollection allWi = wis.Query("SELECT * FROM WorkItems WHERE [State] = 'Active' OR [State] = 'Fixed'");
                foreach (WorkItem item in allWi)
                {
                    List<string> peopleInWorkItem = new List<string>();
                    peopleInWorkItem.Add(item.Fields[CoreField.AssignedTo].Value.ToString());
                    foreach (Revision revision in item.Revisions)
                    {
                        peopleInWorkItem.Add(revision.Fields[CoreField.ChangedBy].Value.ToString());
                        peopleInWorkItem.Add(revision.Fields[CoreField.AssignedTo].Value.ToString());
                    }
                    if (peopleInWorkItem.Contains(_username))
                        dynamicFriend.AddRange(peopleInWorkItem);
                }

            }

            return dynamicFriend.Distinct().ToArray();
        }

        protected SCollection[] GetInteractiveFriends()
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
                        people.Add(revision.Fields[CoreField.ChangedBy].Value.ToString());
                        people.Add(revision.Fields[CoreField.AssignedTo].Value.ToString());
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
