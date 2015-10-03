<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Home</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Home
    </h2>
    <table>
        <tr>
            <th>
                Registered Users</th>
            <th>
                Registered Users/Service</th>
        </tr>
        <tr>
            <td>
                <asp:Chart ID="RegisteredUser" runat="server" Width="450">
                    <Series>
                        <asp:Series Name="UsersSE" ChartType="Pie" Legend="UsersLE" IsValueShownAsLabel="True" >
                        </asp:Series>
                    </Series>
                    <ChartAreas>
                        <asp:ChartArea Name="UsersCA" Area3DStyle-Enable3D="True">
                        </asp:ChartArea>
                    </ChartAreas>
                    <Legends>
                        <asp:Legend Name="UsersLE" >
                        </asp:Legend>
                    </Legends>
                    <Titles>
                        <asp:Title Name="Registered Users">
                        </asp:Title>
                    </Titles>
                </asp:Chart>
            </td>
            <td>
                <asp:Chart ID="RegisteredService" runat="server" Width="450px">
                    <Series>
                        <asp:Series ChartType="Pie" Name="ServicesSE" Legend="ServicesLE" IsValueShownAsLabel="True">
                        </asp:Series>
                    </Series>
                    <ChartAreas>
                        <asp:ChartArea Name="ServicesCA" Area3DStyle-Enable3D="True">
                        </asp:ChartArea>
                    </ChartAreas>
                    <Legends>
                        <asp:Legend Name="ServicesLE" >
                        </asp:Legend>
                    </Legends>
                    <Titles>
                        <asp:Title Name="Registered Services">
                        </asp:Title>
                    </Titles>
                </asp:Chart>
            </td>
        </tr>
    </table>
</asp:Content>
