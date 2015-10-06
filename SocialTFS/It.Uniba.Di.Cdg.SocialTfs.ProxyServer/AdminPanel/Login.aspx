<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="Styles/Site.css" rel="stylesheet" type="text/css" />
    <title>Login</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Login</h2>
    <p class="error" id="errorLB" runat="server">
    </p>
    <fieldset class="login">
        <legend>Authentication</legend>
        <table>
            <tr>
                <td>
                    <label for="UsernameTB">
                        Username</label>
                </td>
                <td>
                    <input type="text" id="UsernameTB" class="field" runat="server" placeholder="Username"
                        tabindex="1" />
                </td>
            </tr>
            <tr>
                <td>
                    <label for="PasswordTB">
                        Password</label>
                </td>
                <td>
                    <input type="password" id="PasswordTB" class="field" runat="server" placeholder="Password"
                        tabindex="2" />
                </td>
            </tr>
            <tr>
                <td>
                </td>
                <td>
                    <a href="PasswordRecovering.aspx" tabindex="4">Forgot your password?</a>
                </td>
            </tr>
            <tr>
                <td>
                </td>
                <td>
                    <input type="submit" value="Login" class="button" tabindex="3" />
                </td>
            </tr>
        </table>
    </fieldset>
</asp:Content>
