<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="Weights.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.Weights" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Weights</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            if ($("#MainContent_WeightTable tr").length > 1) {
                $("#MainContent_WeightTable").show();
                $("#MainContent_Submit").show();
            }

            $("#MainContent_Submit").click(function () {
                $(this).attr("disabled", "disabled");
                var valid = true;
                var body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><weights>";

                $("#MainContent_WeightTable tr").each(function (index) {
                    if (index != 0 && valid) {
                        body += "<item>";
                        $(this).children().each(function (internal_index) {
                            if (internal_index == 0)
                                body += "<service>" + $(this).text() + "</service>";
                            else if (internal_index == 1)
                                body += "<feature>" + $(this).text() + "</feature>";
                            else if (internal_index == 2)
                                if (isNaN($(this).text()))
                                    valid = false;
                                else
                                    body += "<weight>" + $(this).text() + "</weight>";
                        });
                        body += "</item>";
                    }
                });

                body += "</weights>";

                if (valid) {
                    $.ajax({
                        url: "Weights.aspx",
                        type: "POST",
                        contentType: "text/xml",
                        processData: false,
                        data: body,
                        async: false,
                        success: function (data) {
                            if ($(data).find("Saved").first().text() == "true") {
                                $("#MainContent_ErrorPA").attr("class", "confirm");
                                $("#MainContent_ErrorPA").text("Data stored.");
                                $("#MainContent_ErrorPA").show();
                            } else {
                                $("#MainContent_ErrorPA").attr("class", "error");
                                $("#MainContent_ErrorPA").text("Something was wrong. Check your data and try again.");
                                $("#MainContent_ErrorPA").show();
                            }
                        } 
                    });
                }
                else {
                    $("#MainContent_ErrorPA").attr("class", "error");
                    $("#MainContent_ErrorPA").text("Something was wrong. Check your data and try again.");
                    $("#MainContent_ErrorPA").show();
                }
                $(this).removeAttr("disabled");
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Weights
    </h2>
    <p runat="server" id="ErrorPA" />
    <table id="WeightTable" class="data" runat="server" style="display: none">
        <tr>
            <th>
                Service
            </th>
            <th>
                Feature
            </th>
            <th>
                Weights (editable)
            </th>
        </tr>
    </table>
    <input id="Submit" type="button" class="button" runat="server" value="Save"
        tabindex="1" style="display: none" />
</asp:Content>
