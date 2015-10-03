<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="Services.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.Services" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Services</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            if ($("#MainContent_ServiceTable tr").length > 1) {
                $("#MainContent_ServiceTable").show();
            }

            $(".edit").click(function () {
                window.location = "EditService.aspx?id=" + $(this).val();
            });

            $(".delete").click(function () {
                confirmed = confirm("Are you sure?");
                if (confirmed) {
                    idnum = $(this).val();
                    $.post("Services.aspx", { id: idnum },
                        function (data) {
                            if ($(data).find("Deleted").first().text() == "true") {
                                $("#MainContent_Row" + idnum).remove();
                            } else {
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
        Services
    </h2>
    <p runat="server" id="ErrorPA" />
    <table id="ServiceTable" class="data" runat="server" style="display: none">
        <tr>
            <th>
                Name
            </th>
            <th>
                Service
            </th>
            <th>
                Host
            </th>
            <th>
                Edit
            </th>
            <th>
                Delete
            </th>
        </tr>
    </table>
    <input id="NewService" type="button" class="button" runat="server" value="New Service"
        tooltip="Add a new service" onclick="window.location='NewService.aspx'" tabindex="1" />
</asp:Content>
