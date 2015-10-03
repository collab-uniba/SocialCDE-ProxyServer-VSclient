<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="Users.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.Users" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Users</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            if ($("#MainContent_UserTable tr").length > 1) {
                $("#MainContent_AlphabetTable").show();
                $("#MainContent_UserTable").show();
                $("#MainContent_PageTable").show();
            }

            $(".letters").click(function () {
                parts = $(this).attr("id").split('_');
                window.location = "Users.aspx?page=" + parts[1];
            });

            $(".pages").click(function () {
                window.location = "Users.aspx?page=" + $(this).val();
            });

            $(".delete").click(function () {
                confirmed = confirm("Are you sure?");
                if (confirmed) {
                    idnum = $(this).val();
                    $.post("Users.aspx", { id: idnum },
                        function (data) {
                            if ($(data).find("Deleted").first().text() == "true") {
                                window.location = window.location;
                            } else {
                                alert( "" + $(data).find("Errors").first().text());
                                alert("Something was wrong. Please try again later.");
                            }
                        });
                }
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Users
    </h2>
    <p runat="server" id="ErrorPA" />
    <table id="AlphabetTable" class="alphabet" runat="server" style="display: none">
        <tr id="AlphabetRow">
        </tr>
    </table>
    <table id="UserTable" class="data" runat="server" style="display: none">
        <tr>
            <th>
                Username
            </th>
            <th>
                Email
            </th>
            <th>
                Active
            </th>
            <th>
                Posts
            </th>
            <th>
                Followings
            </th>
            <th>
                Followers
            </th>
            <th>
                Delete
            </th>
        </tr>
    </table>
    <table id="PageTable" runat="server" style="display: none">
        <tr id="PageRow">
        </tr>
    </table>
    <input id="NewUser" type="button" class="button" runat="server" value="New User"
        tooltip="Add a new user" onclick="window.location='NewUser.aspx'" tabindex="1" />
</asp:Content>
