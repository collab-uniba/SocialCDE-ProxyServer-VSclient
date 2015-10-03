<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="NewUser.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.NewUser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>New User</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <script type="text/javascript">
        var emails = new Array();

        function submitEmail(email) {
            atpos = email.indexOf("@");
            dotpos = email.lastIndexOf(".");
            if (atpos < 1 || dotpos < atpos + 2 || dotpos + 2 >= email.length) {
                $("#MainContent_ErrEmailRW").show();
            }
            else {
                $("#MainContent_ErrEmailRW").hide();
                if ($("#MainContent_MailTable tr").length == 0)
                    $("#MainContent_ListRW").show();
                $("#MainContent_MailTable").append("<tr><td><input type='text' class='field' disabled='disabled' value='" + email + "' /></td><td><input type='button' class='delete' value='" + email + "' /></td></tr>");
                emails.push(email);
                $("#MainContent_EmailTB").val("");
            }
        }

        $(document).ready(function () {
            $("#MainContent_Update").live('click', function (event) {
                $("#MainContent_Update").attr("disabled", "disabled");
                var body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><users>";
                for (i = 0; i < emails.length; i++)
                    body += "<user>" + emails[i] + "</user>";
                body += "</users>";
                $.ajax({
                    url: "NewUser.aspx",
                    type: "POST",
                    contentType: "text/xml",
                    processData: false,
                    data: body,
                    async: false,
                    success: function (data) {
                        if ($(data).find("NotSent").length == 0) {
                            window.location = "Users.aspx";
                        } else {
                            message = "Something was wrong when I sent emails to:\n";
                            $(data).find("NotSent").each(function () {
                                message += $(this).text() + "\n";
                            });
                            message += "Please try again later.";
                            alert(message);
                            window.location = "Users.aspx";
                        }
                    } 
                });
            });

            $("#MainContent_EmailTB").keypress(function (event) {
                if (event.which == 13) {
                    event.preventDefault();
                    submitEmail($("#MainContent_EmailTB").val());
                }
            });

            $("#Add").click(function () {
                submitEmail($("#MainContent_EmailTB").val())
            });

            $(".delete").live('click', function () {
                $(this).parent().parent().remove();
                if ($("#MainContent_MailTable tr").length == 0)
                    $("#MainContent_ListRW").hide();
                var temp = new Array();
                for (i = 0; i < emails.length; i++) {
                    if ($(this).val() == emails[i]) {
                        temp = temp.concat(emails.slice(i + 1));
                        break;
                    }
                    else
                        temp.push(emails[i]);
                }
                emails = temp;
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        New User</h2>
    <div class="new">
        <fieldset>
            <legend>New User</legend>
            <input id="Id" type="hidden" runat="server" />
            <table id="DataTable" runat="server">
                <tr id="EmailRW" runat="server">
                    <td class="label">
                        <label for="EmailTB">
                            Email:
                        </label>
                    </td>
                    <td>
                        <input type="text" id="EmailTB" class="field" runat="server" maxlength="50" placeholder="User's email"
                            autocomplete="off" tabindex="1" />
                    </td>
                    <td>
                        <input type="button" id="Add" class="button" value="Add" tabindex="2" />
                    </td>
                </tr>
                <tr id="ErrEmailRW" style="display: none">
                    <td colspan="2">
                        <p id="EmailER" class="error">
                            Please, insert a valid email address.</p>
                    </td>
                </tr>
                <tr id="ListRW" runat="server" style="display: none">
                    <td class="label">
                        <label>
                            List:
                        </label>
                    </td>
                    <td colspan="2">
                        <table id="MailTable" runat="server">
                        </table>
                    </td>
                </tr>
            </table>
            <input type="button" id="Update" class="button" runat="server" value="Ok" tabindex="3" />
        </fieldset>
    </div>
</asp:Content>
