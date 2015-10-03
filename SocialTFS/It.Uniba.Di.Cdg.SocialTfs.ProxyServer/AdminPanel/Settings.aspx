<%@ Page Title="" Language="C#" MasterPageFile="~/AdminPanel/Site.Master" AutoEventWireup="true"
    CodeBehind="Settings.aspx.cs" Inherits="It.Uniba.Di.Cdg.SocialTfs.ProxyServer.AdminPanel.Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Settings</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <script type="text/javascript">
        var emptyUsername;
        var usernameExists;
        var emptyEmail;
        var emailExists;
        var invalidEmail;
        var emptyPassword;
        var passwordNotMatch;
        var emptySmtpServer;
        var emptySmtpPort;
        var portNotNumber;
        var emptyAddress;
        var invalidAddress;
        var emptyMailPassword;
        var mailPasswordNotMatch;

        function checkUsername() {
            if ($("#AdminSettingsFS").is(":visible")) {
                if ($("#MainContent_AdminUsernameTB").val() == "") {
                    emptyEmail = true;
                    $("#ErrAdminUsernameRW").show();
                    $("#AdminUsernameER").html("Username should not be empty.");
                }
                else {
                    emptyEmail = false;
                    $("#ErrAdminUsernameRW").hide();
                    $.get("Settings.aspx?username=" + $("#MainContent_AdminUsernameTB").val(), function (data) {
                        if ($(data).find("IsAviable").first().text() == "true") {
                            usernameExists = false;
                            $("#ErrAdminUsernameRW").hide();
                        }
                        else {
                            usernameExists = true;
                            $("#ErrAdminUsernameRW").show();
                            $("#AdminUsernameER").html("Username already exists. Please, choose another one.");
                        }
                    });
                }
            }
        }

        function checkEmail() {
            if ($("#AdminSettingsFS").is(":visible")) {
                if ($("#MainContent_AdminEmailTB").val() == "") {
                    emptyEmail = true;
                    $("#ErrAdminEmailRW").show();
                    $("#AdminEmailER").html("Email should not be empty.");
                }
                else {
                    emptyEmail = false;
                    $("#ErrAdminEmailRW").hide();
                    if (validEmail($("#MainContent_AdminEmailTB").val())) {
                        invalidEmail = false;
                        $("#ErrAdminEmailRW").hide();
                        $.get("Settings.aspx?email=" + $("#MainContent_AdminEmailTB").val(), function (data) {
                            if ($(data).find("IsAviable").first().text() == "true") {
                                emailExists = false;
                                $("#ErrAdminEmailRW").hide();
                            }
                            else {
                                emailExists = true;
                                $("#ErrAdminEmailRW").show();
                                $("#AdminEmailER").html("Email already exists. Please, choose another one.");
                            }
                        });
                    }
                    else {
                        invalidEmail = true;
                        $("#ErrAdminEmailRW").show();
                        $("#AdminEmailER").html("Please, insert a valid email address.");
                    }
                }
            }
        }

        function checkPassword() {
            if ($("#AdminSettingsFS").is(":visible") && $("#PasswordRW").is(":visible")) {
                if ($("#MainContent_PasswordTB").val() == "") {
                    emptyPassword = true;
                    $("#ErrPasswordRW").show();
                    $("#PasswordER").html("Passwords should not be empty.");
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
                    $("#ConfirmER").html("Passwords do not match.");
                }
                else {
                    passwordNotMatch = false;
                    $("#ErrConfirmRW").hide();
                }
            }
        }

        function checkSmtpServer() {
            if ($("#MailSettingsFS").is(":visible")) {
                if ($("#MainContent_SmtpServerTB").val() == "") {
                    emptySmtpServer = true;
                    $("#ErrSmtpServerRW").show();
                    $("#SmtpServerER").html("Server should not be empty.");
                }
                else {
                    emptySmtpServer = false;
                    $("#ErrSmtpServerRW").hide();
                }
            }
        }

        function checkSmtpPort() {
            if ($("#MailSettingsFS").is(":visible")) {
                if ($("#MainContent_SmtpPortTB").val() == "") {
                    emptySmtpPort = true;
                    $("#ErrSmtpPortRW").show();
                    $("#SmtpPortER").html("Port should not be empty.");
                }
                else {
                    emptySmtpPort = false;
                    $("#ErrSmtpPortRW").hide();
                    if (isNaN($("#MainContent_SmtpPortTB").val())) {
                        portNotNumber = true;
                        $("#ErrSmtpPortRW").show();
                        $("#SmtpPortER").html("Please, insert a number.");
                    }
                    else {
                        portNotNumber = false;
                        $("#ErrSmtpPortRW").hide();
                    }
                }
            }
        }

        function checkMailAddress() {
            if ($("#MailSettingsFS").is(":visible")) {
                if ($("#MainContent_MailAddressTB").val() == "") {
                    emptyAddress = true;
                    $("#ErrMailAddressRW").show();
                    $("#MailAddressER").html("Address should not be empty.");
                }
                else {
                    emptyAddress = false;
                    $("#ErrMailAddressRW").hide();
                    if (validEmail($("#MainContent_MailAddressTB").val())) {
                        invalidAddress = false;
                        $("#ErrMailAddressRW").hide();
                    }
                    else {
                        invalidAddress = true;
                        $("#ErrMailAddressRW").show();
                        $("#MailAddressER").html("Please, insert a valid email address.");
                    }
                }
            }
        }

        function checkMailPassword() {
            if ($("#MailSettingsFS").is(":visible") && $("#MailPasswordRW").is(":visible")) {
                if ($("#MainContent_MailPasswordTB").val() == "") {
                    emptyMailPassword = true;
                    $("#ErrMailPasswordRW").show();
                    $("#MailPasswordER").html("Password should not be empty.");
                }
                else {
                    emptyMailPassword = false;
                    $("#ErrMailPasswordRW").hide();
                }
            }
        }

        function checkMailConfirm() {
            if ($("#MailSettingsFS").is(":visible") && $("#MailConfirmRW").is(":visible")) {
                if ($("#MainContent_MailPasswordTB").val() != $("#MainContent_MailConfirmTB").val()) {
                    mailPasswordNotMatch = true;
                    $("#ErrMailConfirmRW").show();
                    $("#MailConfirmER").html("Passwords do not match.");
                }
                else {
                    mailPasswordNotMatch = false;
                    $("#ErrMailConfirmRW").hide();
                }
            }
        }

        function validEmail(email) {
            atpos = email.indexOf("@");
            dotpos = email.lastIndexOf(".");
            if (atpos < 1 || dotpos < atpos + 2 || dotpos + 2 >= email.length)
                return false;
            else
                return true;
        }

        function showAdminSettings() {
            $("#AdminSettingsFS").show();
        }

        function showMailSettings() {
            $("#MailSettingsFS").show();
        }

        function hideAdminSettings() {
            $("#AdminSettingsFS").hide();
            $("#MainContent_ChangePasswordCB").removeAttr("checked");
            $("#PasswordRW").hide();
            $("#ConfirmRW").hide();
            $("#ErrAdminUsernameRW").hide();
            $("#ErrAdminEmailRW").hide();
            $("#ErrPasswordRW").hide();
            $("#ErrConfirmRW").hide();
        }

        function hideMailSettings() {
            $("#MailSettingsFS").hide();
            $("#MainContent_ChangeMailPasswordCB").removeAttr("checked");
            $("#MailPasswordRW").hide();
            $("#MailConfirmRW").hide();
            $("#ErrSmtpServerRW").hide();
            $("#ErrSmtpPortRW").hide();
            $("#ErrMailAddressRW").hide();
            $("#ErrMailPasswordRW").hide();
            $("#ErrMailConfirmRW").hide();
        }

        function checkService() {
            if ($("#MainContent_SettingSE").val() == "Admin settings") {
                hideMailSettings();
                showAdminSettings();
            }
            else if ($("#MainContent_SettingSE").val() == "Mail settings") {
                hideAdminSettings();
                showMailSettings();
            }
            else {
                hideAdminSettings();
                hideMailSettings();
            }
        }

        function checkChangePassword() {
            if ($("#MainContent_ChangePasswordCB").attr("checked") == "checked") {
                $("#PasswordRW").show();
                $("#ConfirmRW").show();
            }
            else {
                $("#PasswordRW").hide();
                $("#ConfirmRW").hide();
                emptyPassword = false;
                $("#ErrPasswordRW").hide();
                passwordNotMatch = false;
                $("#ErrConfirmRW").hide();
            }
        }

        function checkChangeMailPassword() {
            if ($("#MainContent_ChangeMailPasswordCB").attr("checked") == "checked") {
                $("#MailPasswordRW").show();
                $("#MailConfirmRW").show();
            }
            else {
                $("#MailPasswordRW").hide();
                $("#MailConfirmRW").hide();
                emptyMailPassword = false;
                $("#ErrMailPasswordRW").hide();
                mailPasswordNotMatch = false;
                $("#ErrMailConfirmRW").hide();
            }
        }

        $(document).ready(function () {
            $("#MainContent_SettingSE").val("");
            hideAdminSettings();
            hideMailSettings();
            emptyUsername = false;
            usernameExists = false;
            emptyEmail = false;
            emailExists = false;
            invalidEmail = false;
            emptyPassword = false;
            passwordNotMatch = false;
            emptySmtpServer = false;
            emptySmtpPort = false;
            portNotNumber = false;
            emptyAddress = false;
            invalidAddress = false;
            emptyMailPassword = false;
            mailPasswordNotMatch = false;

            $("#MainContent_AdminUsernameTB").focusout(function () { checkUsername(); });

            $("#MainContent_AdminEmailTB").focusout(function () { checkEmail(); });

            $("#MainContent_PasswordTB").focusout(function () { checkPassword(); });

            $("#MainContent_ConfirmTB").focusout(function () { checkConfirm(); });

            $("#MainContent_SmtpServerTB").focusout(function () { checkSmtpServer(); });

            $("#MainContent_SmtpPortTB").focusout(function () { checkSmtpPort(); });

            $("#MainContent_MailAddressTB").focusout(function () { checkMailAddress(); });

            $("#MainContent_MailPasswordTB").focusout(function () { checkMailPassword(); });

            $("#MainContent_MailConfirmTB").focusout(function () { checkMailConfirm(); });

            $("#MainContent_SettingSE").keyup(function () { checkService(); });

            $("#MainContent_SettingSE").change(function () { checkService(); });

            $("#MainContent_AdminSubmit").click(function () {
                $("#MainContent_AdminSubmit").attr("disabled", "disabled");
                checkUsername();
                checkEmail();
                checkPassword();
                checkConfirm();
                if (!emptyUsername && !usernameExists && !emptyEmail && !emailExists && !invalidEmail
                && ($("#MainContent_ChangePasswordCB").attr("checked") != "checked" || (!emptyPassword && !passwordNotMatch))) {
                    $("#ctl01").submit();
                }
                else {
                    $("#MainContent_ErrorPA").attr("class", "error");
                    $("#MainContent_ErrorPA").html("Something was wrong. Please, check data and try again.");
                }
                $("#MainContent_AdminSubmit").removeAttr("disabled");
            });

            $("#MainContent_MailSubmit").click(function () {
                $("#MainContent_MailSubmit").attr("disabled", "disabled");
                checkSmtpServer();
                checkSmtpPort();
                checkMailAddress();
                checkMailPassword();
                checkMailConfirm();
                if (!emptySmtpServer && !emptySmtpPort && !portNotNumber && !emptyAddress && !invalidAddress
                && ($("#MainContent_ChangeMailPasswordCB").attr("checked") != "checked" || (!emptyMailPassword && !mailPasswordNotMatch))) {
                    $("#ctl01").submit();
                }
                else {
                    $("#MainContent_ErrorPA").attr("class", "error");
                    $("#MainContent_ErrorPA").html("Something was wrong. Please, check data and try again.");
                }
                $("#MainContent_MailSubmit").removeAttr("disabled");
            });

            $("#MainContent_AdminReset").click(function () { hideAdminSettings(); });

            $("#MainContent_MailReset").click(function () { hideMailSettings(); });

            $("#MainContent_ChangePasswordCB").change(function () { checkChangePassword(); });

            $("#MainContent_ChangeMailPasswordCB").change(function () { checkChangeMailPassword(); });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Settings</h2>
    <div class="new">
        <p runat="server" id="ErrorPA" />
        <fieldset>
            <legend>Settings</legend>
            <table>
                <tr>
                    <td class="label">
                        <label for="MainContent_SettingSE">
                            Settings</label>
                    </td>
                    <td>
                        <select id="SettingSE" class="field" runat="server" tabindex="1">
                            <option value="" />
                            <option value="Admin settings" />
                            <option value="Mail settings" />
                        </select>
                    </td>
                </tr>
            </table>
        </fieldset>
        <fieldset id="AdminSettingsFS">
            <legend>Admin Settings</legend>
            <table>
                <tr>
                    <td class="label">
                        <label for="MainContent_AdminUsernameTB">
                            Username</label>
                    </td>
                    <td>
                        <input id="AdminUsernameTB" type="text" runat="server" class="field" placeholder="Admin username"
                            tabindex="2" />
                    </td>
                </tr>
                <tr id="ErrAdminUsernameRW" style="display: none">
                    <td colspan="2">
                        <p id="AdminUsernameER" class="error" />
                    </td>
                </tr>
                <tr>
                    <td class="label">
                        <label for="MainContent_AdminEmailTB">
                            Email</label>
                    </td>
                    <td>
                        <input id="AdminEmailTB" type="text" runat="server" class="field" placeholder="Admin email"
                            tabindex="3" />
                    </td>
                </tr>
                <tr id="ErrAdminEmailRW" style="display: none">
                    <td colspan="2">
                        <p id="AdminEmailER" class="error" />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <input id="ChangePasswordCB" type="checkbox" runat="server" tabindex="4" />
                        <label for="MainContent_ChangePasswordCB">
                            Change password</label>
                    </td>
                </tr>
                <tr id="PasswordRW" style="display: none">
                    <td class="label">
                        <label for="MainContent_PasswordTB">
                            Password</label>
                    </td>
                    <td>
                        <input id="PasswordTB" type="password" runat="server" class="field" placeholder="Admin password"
                            tabindex="5" />
                    </td>
                </tr>
                <tr id="ErrPasswordRW" style="display: none">
                    <td colspan="2">
                        <p id="PasswordER" class="error" />
                    </td>
                </tr>
                <tr id="ConfirmRW" style="display: none">
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
                        <p id="ConfirmER" class="error" />
                    </td>
                </tr>
            </table>
            <input id="AdminSubmit" type="button" value="Save" runat="server" class="button"
                tabindex="7" />
            <input id="AdminReset" type="reset" value="Reset" runat="server" class="button" tabindex="8" />
        </fieldset>
        <fieldset id="MailSettingsFS">
            <legend>Mail Settings</legend>
            <table class="new">
                <tr>
                    <td class="label">
                        <label for="MainContent_SmtpServerTB">
                            Server</label>
                    </td>
                    <td>
                        <input id="SmtpServerTB" type="text" runat="server" class="field" placeholder="SMTP server"
                            tabindex="9" />
                    </td>
                </tr>
                <tr id="ErrSmtpServerRW" style="display: none">
                    <td colspan="2">
                        <p id="SmtpServerER" class="error" />
                    </td>
                </tr>
                <tr>
                    <td class="label">
                        <label for="MainContent_SmtpPortTB">
                            Port</label>
                    </td>
                    <td>
                        <input id="SmtpPortTB" type="text" runat="server" class="field" placeholder="SMTP port"
                            tabindex="10" />
                    </td>
                </tr>
                <tr id="ErrSmtpPortRW" style="display: none">
                    <td colspan="2">
                        <p id="SmtpPortER" class="error" />
                    </td>
                </tr>
                <tr>
                    <td class="label">
                        <label for="MainContent_SmtpSecuritySE">
                            Security</label>
                    </td>
                    <td>
                        <select id="SmtpSecuritySE" runat="server" class="field" tabindex="11">
                            <option title="None" value="None" />
                            <option title="SSL/TLS" value="SSL/TLS" />
                            <option title="STARTTLS" value="STARTTLS" />
                        </select>
                    </td>
                </tr>
                <tr>
                    <td class="label">
                        <label for="MainContent_MailAddressTB">
                            Address</label>
                    </td>
                    <td>
                        <input id="MailAddressTB" type="text" runat="server" class="field" placeholder="Email sender address"
                            tabindex="12" />
                    </td>
                </tr>
                <tr id="ErrMailAddressRW" style="display: none">
                    <td colspan="2">
                        <p id="MailAddressER" class="error" />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <input id="ChangeMailPasswordCB" type="checkbox" runat="server" tabindex="13" />
                        <label for="MainContent_ChangeMailPasswordCB">
                            Change password</label>
                    </td>
                </tr>
                <tr id="MailPasswordRW" style="display: none">
                    <td class="label">
                        <label for="MainContent_MailPasswordTB">
                            Password</label>
                    </td>
                    <td>
                        <input id="MailPasswordTB" type="password" runat="server" class="field" placeholder="Sender password"
                            tabindex="14" />
                    </td>
                </tr>
                <tr id="ErrMailPasswordRW" style="display: none">
                    <td colspan="2">
                        <p id="MailPasswordER" class="error" />
                    </td>
                </tr>
                <tr id="MailConfirmRW" style="display: none">
                    <td class="label">
                        <label for="MainContent_MailConfirmTB">
                            Confirm</label>
                    </td>
                    <td>
                        <input id="MailConfirmTB" type="password" runat="server" class="field" placeholder="Confirm password"
                            tabindex="15" />
                    </td>
                </tr>
                <tr id="ErrMailConfirmRW" style="display: none">
                    <td colspan="2">
                        <p id="MailConfirmER" class="error" />
                    </td>
                </tr>
            </table>
            <input id="MailSubmit" type="button" value="Save" runat="server" class="button" tabindex="16" />
            <input id="MailReset" type="reset" value="Reset" runat="server" class="button" tabindex="17" />
        </fieldset>
    </div>
</asp:Content>
