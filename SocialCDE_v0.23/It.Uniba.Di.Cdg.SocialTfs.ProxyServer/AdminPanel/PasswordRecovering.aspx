<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="PasswordRecovering.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.PasswordRecovering" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Password Recovery</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <script type="text/javascript">
        var emptyPassword;
        var passwordNotMatch;

        function checkPassword() {
            if ($("#AdminSettingsFS").is(":visible") && $("#PasswordRW").is(":visible")) {
                if ($("#MainContent_PasswordTB").val() == "") {
                    emptyPassword = true;
                    $("#ErrPasswordRW").show();
                }
                else {
                    emptyPassword = false;
                    $("#ErrPasswordRW").hide();
                }
            }
        }

        function checkConfirm() {
            if ($("#AdminSettingsFS").is(":visible") && $("#ConfirmRW").is(":visible")) {
                if ($("#MainContent_PasswordTB").val() != $("#MainContent_ConfirmTB").val()) {
                    passwordNotMatch = true;
                    $("#ErrConfirmRW").show();
                }
                else {
                    passwordNotMatch = false;
                    $("#ErrConfirmRW").hide();
                }
            }
        }

        function hideAll() {
            emptyPassword = false;
            $("#ErrPasswordRW").hide();
            passwordNotMatch = false;
            $("#ErrConfirmRW").hide();
        }

        $(document).ready(function () {
            emptyPassword = false;
            passwordNotMatch = false;

            $("#MainContent_PasswordTB").focusout(function () { checkPassword(); });

            $("#MainContent_ConfirmTB").focusout(function () { checkConfirm(); });

            $("#MainContent_Submit").click(function () {
                $("#MainContent_Submit").attr("disabled", "disabled");
                checkPassword();
                checkConfirm();
                if (!emptyPassword && !passwordNotMatch) {
                    $("#ctl01").submit();
                }
                else {
                    $("#MainContent_ErrorPA").attr("class", "error");
                    $("#MainContent_ErrorPA").html("Something was wrong. Please, check data and try again.");
                }
                $("#MainContent_AdminSubmit").removeAttr("disabled");
            });

            $("#MainContent_Reset").click(function () { hideAll(); });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Password Recovery</h2>
    <div class="new">
        <p runat="server" id="ErrorPA" />
        <fieldset id="AdminSettingsFS">
            <legend>Password Recovery</legend>
            <table>
                <tr id="PasswordRW">
                    <td class="label">
                        <label for="MainContent_PasswordTB">
                            Password</label>
                    </td>
                    <td>
                        <input id="PasswordTB" type="password" runat="server" class="field" placeholder="New password"
                            tabindex="5" />
                    </td>
                </tr>
                <tr id="ErrPasswordRW" style="display: none">
                    <td colspan="2">
                        <p id="PasswordER" class="error" >Passwords should not be empty.</p>
                    </td>
                </tr>
                <tr id="ConfirmRW">
                    <td class="label">
                        <label for="MainContent_ConfirmTB">
                            Confirm</label>
                    </td>
                    <td>
                        <input id="ConfirmTB" type="password" runat="server" class="field" placeholder="Confirm password"
                            tabindex="6" />
                    </td>
                </tr>
                <tr id="ErrConfirmRW" style="display: none">
                    <td colspan="2">
                        <p id="ConfirmER" class="error" >Passwords do not match.</p>
                    </td>
                </tr>
            </table>
            <input id="Submit" type="button" value="Save" runat="server" class="button"
                tabindex="7" />
            <input id="Reset" type="reset" value="Reset" runat="server" class="button" tabindex="8" />
        </fieldset>
    </div>
</asp:Content>
