<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="NewService.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.NewService" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>New Service</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <script type="text/javascript">
        var emptyName;
        var emptyHost;
        var emptyKey;
        var emptySecret;

        function checkName() {
            if ($("#MainContent_NameRW").is(":visible")) {
                if ($("#MainContent_NameTB").val() == "") {
                    emptyName = true;
                    $("#MainContent_ErrNameRW").show();
                }
                else {
                    emptyName = false;
                    $("#MainContent_ErrNameRW").hide();
                }
            }
        }

        function checkHost() {
            if ($("#MainContent_HostRW").is(":visible")) {
                if ($("#MainContent_HostTB").val() == "") {
                    emptyHost = true;
                    $("#MainContent_ErrHostRW").show();
                    $("#HostER").html("Service host should not be empty.");
                }
                else {
                    emptyHost = false;
                    $("#MainContent_ErrHostRW").hide();
                }
            }
        }

        function checkKey() {
            if ($("#MainContent_ConsumerKeyRW").is(":visible")) {
                if ($("#MainContent_ConsumerKeyTB").val() == "") {
                    emptyKey = true;
                    $("#MainContent_ErrConsumerKeyRW").show();
                }
                else {
                    emptyKey = false;
                    $("#MainContent_ErrConsumerKeyRW").hide();
                }
            }
        }

        function checkSecret() {
            if ($("#MainContent_ConsumerSecretRW").is(":visible")) {
                if ($("#MainContent_ConsumerSecretTB").val() == "") {
                    emptySecret = true;
                    $("#MainContent_ErrConsumerSecretRW").show();
                }
                else {
                    emptySecret = false;
                    $("#MainContent_ErrConsumerSecretRW").hide();
                }
            }
        }

        function checkService() {
            $.get("NewService.aspx?id=" + $("#MainContent_ServiceSE").val(), function (data) {
                if ($(data).find("CanHaveMoreInstance").first().text() == "true") {
                    $("#MainContent_NameRW").show();
                    $("#MainContent_HostRW").show();
                    if ($(data).find("NeedOAuth").first().text() == "true") {
                        $("#MainContent_ConsumerKeyRW").show();
                        $("#MainContent_ConsumerSecretRW").show();
                    }


                    else {
                        $("#MainContent_ConsumerKeyRW").hide();
                        $("#MainContent_ConsumerSecretRW").hide();
                        emptyKey = false;
                        $("#MainContent_ErrConsumerKeyRW").hide();
                        emptySecret = false;
                        $("#MainContent_ErrConsumerSecretRW").hide();
                        $("#MainContent_GitHubLabelRW").hide();
                        $("#MainContent_MessageGitHubLabelRW").hide();
                    }
                }
                else if ($(data).find("NeedGitHubLabel").first().text() == "true") {
                    $("#MainContent_GitHubLabelRW").show();
                    $("#MainContent_MessageGitHubLabelRW").show();
                }
                else {
                    hideAll();
                }
            });
        }

        function hideAll() {
            $("#MainContent_NameRW").hide();
            $("#MainContent_HostRW").hide();
            $("#MainContent_ConsumerKeyRW").hide();
            $("#MainContent_ConsumerSecretRW").hide();
            emptyName = false;
            $("#MainContent_ErrNameRW").hide();
            emptyHost = false;
            $("#MainContent_ErrHostRW").hide();
            emptyKey = false;
            $("#MainContent_ErrConsumerKeyRW").hide();
            emptySecret = false;
            $("#MainContent_ErrConsumerSecretRW").hide();
            $("#MainContent_GitHubLabelRW").hide();
            $("#MainContent_MessageGitHubLabelRW").hide();
        }

        $(document).ready(function () {
            emptyName = false;
            emptyHost = false;
            emptyKey = false;
            emptySecret = false;

            $("#MainContent_ServiceSE").keyup(function () { checkService(); });

            $("#MainContent_ServiceSE").change(function () { checkService(); });

            $("#MainContent_Submit").click(function () {
                $("#MainContent_Submit").attr("disabled", "disabled");
                checkName();
                checkHost();
                checkKey();
                checkSecret();

                if (!emptyName && !emptyHost && !emptyKey && !emptySecret)
                    $("#ctl01").submit();
                else
                    $("#MainContent_ErrorPA").show();
                $("#MainContent_Submit").removeAttr("disabled");
            });

            $("#MainContent_Reset").click(function () { hideAll(); });

            $("#MainContent_NameTB").focusout(function () { checkName(); });

            $("#MainContent_HostTB").focusout(function () { checkHost(); });

            $("#MainContent_ConsumerKeyTB").focusout(function () { checkKey(); });

            $("#MainContent_ConsumerSecretTB").focusout(function () { checkSecret(); });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        New Service</h2>
    <div class="new">
        <fieldset>
            <p runat="server" id="ErrorPA" class="error" style="display: none">
                Something was wrong. Check your data and try again.</p>
            <legend>New Service</legend>
            <table id="DataTable" runat="server">
                <tr id="ServiceRW" runat="server">
                    <td class="label">
                        <label for="ServiceTB">
                            Service:
                        </label>
                    </td>
                    <td>
                        <select id="ServiceSE" name="ServiceSE" class="field" runat="server" tabindex="1" />
                    </td>
                </tr>
                <tr id="NameRW" runat="server" style="display: none">
                    <td class="label">
                        <label for="NameTB">
                            Name:
                        </label>
                    </td>
                    <td>
                        <input type="text" id="NameTB" name="NameTB" class="field" runat="server" maxlength="50"
                            autocomplete="off" placeholder="Service name" tabindex="2" />
                    </td>
                </tr>
                <tr id="ErrNameRW" style="display: none">
                    <td colspan="2">
                        <p class="error">
                            Service name should not be empty.</p>
                    </td>
                </tr>
                <tr id="HostRW" runat="server" style="display: none">
                    <td class="label">
                        <label for="HostTB">
                            Host:
                        </label>
                    </td>
                    <td>
                        <input type="text" id="HostTB" name="HostTB" class="field" runat="server" maxlength="100"
                            autocomplete="off" placeholder="Service host" tabindex="3" />
                    </td>
                </tr>
                <tr id="ErrHostRW" style="display: none">
                    <td colspan="2">
                        <p id="HostER" class="error" />
                    </td>
                </tr>
                <tr id="ConsumerKeyRW" runat="server" style="display: none">
                    <td class="label">
                        <label for="ConsumerKeyTB">
                            Key:
                        </label>
                    </td>
                    <td>
                        <input type="text" id="ConsumerKeyTB" name="ConsumerKeyTB" class="field" runat="server"
                            maxlength="50" autocomplete="off" placeholder="Service consumer key for SocialTFS"
                            tabindex="5" />
                    </td>
                </tr>
                <tr id="ErrConsumerKeyRW" style="display: none">
                    <td colspan="2">
                        <p class="error">
                            Consumer key should not be empty.</p>
                    </td>
                </tr>
                <tr id="ConsumerSecretRW" runat="server" style="display: none">
                    <td class="label">
                        <label for="ConsumerSecretTB">
                            Secret:
                        </label>
                    </td>
                    <td>
                        <input type="text" id="ConsumerSecretTB" name="ConsumerSecretTB" class="field" runat="server"
                            maxlength="50" autocomplete="off" placeholder="Service consumer secret for SocialTFS"
                            tabindex="6" />
                    </td>
                </tr>
                <tr id="ErrConsumerSecretRW" style="display: none">
                    <td colspan="2">
                        <p class="error">
                            Consumer secret should not be empty.</p>
                    </td>
                </tr>
                <tr id="GitHubLabelRW" runat="server" style="display: none">
                    <td class="label">
                        <label for="GitHubLabelTB">
                            GitHub's Labels:
                        </label>
                    </td>
                    <td>
                         <input type="text" id="GitHubLabelTB" name="GitHubLabelTB" class="field" runat="server"
                            maxlength="50" autocomplete="off" placeholder="GitHub label for SocialTFS"
                            tabindex="7" />
                    </td>
                </tr>
                <tr id="MessageGitHubLabelRW" style="display: none">
                    <td colspan="2">
                        <p class="error">
                           If you want enter more than one label, please enter each label separated by comma.</p>
                    </td>
                </tr>
            </table>
            <input type="button" id="Submit" class="button" runat="server" value="Ok" tabindex="9" />
            <input type="reset" id="Reset" class="button" runat="server" value="Reset" tabindex="10" />
        </fieldset>
    </div>
</asp:Content>
