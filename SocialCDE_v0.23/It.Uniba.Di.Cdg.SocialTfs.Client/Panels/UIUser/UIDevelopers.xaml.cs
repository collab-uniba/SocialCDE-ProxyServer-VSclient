using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using It.Uniba.Di.Cdg.SocialTfs.Client.Objects.UIObject;

namespace It.Uniba.Di.Cdg.SocialTfs.Client.Panels.UIUser
{
    /// <summary>
    /// Logica di interazione per UIPeople.xaml
    /// </summary>
    public partial class UIDevelopers : UserControl
    {
        private SharedLibrary.WUser _user;

        public UIDevelopers(WUser _user)
        {
            InitializeComponent();

            string[] skills = UIController.Proxy.GetSkills(UIController.MyProfile.Username, UIController.Password, _user.Username);
            WEdu[] educations = UIController.Proxy.GetEducations(UIController.MyProfile.Username, UIController.Password, _user.Username);
            WPos[] positions = UIController.Proxy.GetPositions(UIController.MyProfile.Username, UIController.Password, _user.Username);
            WReputation repu = UIController.Proxy.GetReputations(UIController.MyProfile.Username, UIController.Password, _user.Username);
            int metascore = 0;
            int flag = 0;
            if (repu.stackReputationValue != null)
            {
                flag += 5;
                metascore = (int)(repu.stackAnswer * 4 + repu.stackQuestion * 1 + repu.stackBronze * 1 + repu.stackSilver * 2 + repu.stackGold * 4);
            }
            if (repu.ohlohKudoRank != null)
            {
                flag += 2;
                metascore += (int)(repu.ohlohKudoRank * 3 + repu.ohlohKudoScore * 3);
            }
            if (repu.coderwallEndorsements != null)
            {
                flag += 1;
                metascore += (int)(repu.coderwallEndorsements * 3);
            }
            if (flag == 0)
                metascore = 0;
            metascore = metascore / flag;
            

            UpdateHeader(_user.Username, "", "", _user.Avatar, metascore);
            UpdateExperiences(positions);
            UpdateSkillExp(skills);
            UpdateEducations(educations);
            if ( repu != null )
                if (repu.coderwallEndorsements != null || 
                    repu.ohlohKudoScore !=null || 
                    repu.stackReputationValue !=null)
                {
                    UpdateStackOverflowReputation(repu);
                    UpdateOhlohReputation(repu);
                    UpdateCoderwallReputation(repu);
                    UpdateLinkedInReputation(repu);
                }
        }

        private void UpdateHeader(string displayName, string username, string mainJob, string url, int score)
        {
            uiheader.Update(displayName, username, mainJob , url, score);
        }

        private void UpdateExperiences(WPos[] positions)
        {
            uiexperiences.Update(positions);
        }

        private void UpdateEducations(WEdu[] educations)
        {
            uieducations.Update(educations);
        }

        private void UpdateSkillExp(string[] skills)
        {
            uiskills.Update(skills);
        }

        private void UpdateStackOverflowReputation(WReputation repu)
        {

            try
            {
                if (repu.stackReputationValue == null)
                {
                    uistackoverflowreputation.Visibility = System.Windows.Visibility.Collapsed;
                }
                uistackoverflowreputation.Update(repu);
            }
            catch (Exception)
            {
                
               
            }
        }

        private void UpdateOhlohReputation(WReputation repu)
        {
            try
            {
                if (repu.ohlohKudoRank == null)
                {
                    uiohlohreputation.Visibility = System.Windows.Visibility.Collapsed;
                }
                uiohlohreputation.Update(repu);
            }
            catch (Exception)
            {
                
            }
        }

        private void UpdateCoderwallReputation(WReputation repu)
        {
            try
            {
                if (repu.coderwallEndorsements == null)
                {
                    uicoderwallreputation.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    uicoderwallreputation.Visibility = Visibility.Visible;
                    uicoderwallreputation.Update(repu);
                }
            }
            catch (Exception)
            {
                
           
            }
        }

        private void UpdateLinkedInReputation(WReputation repu)
        {
            try
            {
                if (repu.linkedinRecommendations == null)
                {
                    uilinkedinreputation.Visibility = System.Windows.Visibility.Collapsed;
                }
                uilinkedinreputation.Update(repu);
            }
            catch (Exception)
            {
                
             
            }
        }
    }
}
