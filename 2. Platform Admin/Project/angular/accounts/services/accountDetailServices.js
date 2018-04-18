(function () {
    'use strict';

    var serviceId = 'accountDetailServices';
    var urlBase = '/Accounts/Json';
    var accounts = [];

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', accountDetailServices]);

    function accountDetailServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //acoount ---------------------------------
            getAccount: getAccount,
            getAccountCapacity: getAccountCapacity,
            updateAccountName: updateAccountName,

            //provisioning -----------------------
            provisionAccount:provisionAccount,
            
            //Check If Partitions Are Available for Unprovisioned Account -------------------------
            partitionsAvailable: partitionsAvailable,


            //credit card info -----------------------
            getCardInfo: getCardInfo,
            addUpdateCard: addUpdateCard,

            //users -----------------------------------
            getUsers: getUsers,

            inviteUser: inviteUser,
            createUser: createUser,
            deleteUser: deleteUser,

            sendPasswordLink: sendPasswordLink,
            updateUserName: updateUserName,
            updateUserEmail: updateUserEmail,
            updateUserRole: updateUserRole,
            changeUserActiveState: changeUserActiveState,
            changeUserOwnershipStatus: changeUserOwnershipStatus,
            getAllAccountsForEmail: getAllAccountsForEmail,
            //getUserRoles: getUserRoles,

            //invitations -----------------------------
            getInvitations: getInvitations,
            deleteInvitation: deleteInvitation,
            resendInvitation: resendInvitation,

            //password claims -----------------------------
            getPasswordClaims: getPasswordClaims,


            //Logs -----------------------------
            getAccountLog: getAccountLog,
            getAccountLogByCategory: getAccountLogByCategory,
            getAccountLogByActivity: getAccountLogByActivity,
            getAccountLogByUser: getAccountLogByUser,
            getAccountLogByObject: getAccountLogByObject,
            //Log Types
            getAccountLogCategories: getAccountLogCategories,
            getAccountLogActivities: getAccountLogActivities,

            //Close Account -------------------------
            closeUnprovisionedAccount: closeUnprovisionedAccount,
            closeAccount: closeAccount,
            doesAccountRequireClosureApproval: doesAccountRequireClosureApproval,
            approveAccountClosure: approveAccountClosure,
            reversAccounteClosureApproval: reversAccounteClosureApproval,
            reactivateSubscription: reactivateSubscription,
            accelerateAccountClosure: accelerateAccountClosure,

            //DataInjection ------------------------
            injectImageDocuments: injectImageDocuments,


        };

        return service;



        /* ==========================================
          ACCOUNT
        ==========================================*/

        function getAccount(name) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Details/' + name + cacheBuster });
        }


        function updateAccountName(accountId, newName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateAccountName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    newName: newName,
                }
            });
        }

        function getAccountCapacity(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountCapacity?accountId=' + accountId + cacheBuster });
        }


        /* ==========================================
          PROVISIONING
        ==========================================*/

        function provisionAccount(accountId) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ProvisionAccount' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId
                }
            });
        }

        /* ==========================================
              Partitions Available ? (Check before allowing provisioning button to display)
            ==========================================*/

        function partitionsAvailable(searchPlan) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/PartitionsAvailable?searchPlan=' + searchPlan + cacheBuster });
        }



        /* ==========================================
          CREDIT CARD
        ==========================================*/

        function getCardInfo(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetCardInfo/' + accountId + cacheBuster });
        }

        function addUpdateCard(accountId, cardName, cardNumber, cvc, expirationMonth, expirationYear) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/AddUpdateCard' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    cardName: cardName,
                    cardNumber: cardNumber,
                    cvc: cvc,
                    expirationMonth: expirationMonth,
                    expirationYear: expirationYear,
                }
            });
        }

        /* ==========================================
          USERS
        ==========================================*/

        function getUsers(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetUsers/' + accountId + cacheBuster });
        }

        function getAllAccountsForEmail(email) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/GetAllAccountsForEmail' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    email: email,
                }
            });
        }

        function createUser(accountId, email, firstName, lastName, role, isOwner, password, confirmPassword) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    role: role,
                    isOwner: isOwner,
                    password: password,
                    confirmPassword: confirmPassword,
                }
            });
        }

        function deleteUser(accountId, userId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    userId: userId,
                }
            });
        }

        function updateUserName(accountId, userId, firstName, lastName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    userId: userId,
                    firstName: firstName,
                    lastName: lastName
                }
            });
        }

        function updateUserEmail(accountId, userId, email) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateEmail' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    userId: userId,
                    email: email
                }
            });
        }

        function updateUserRole(accountId, userId, role) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateRole' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    userId: userId,
                    role: role
                }
            });
        }

        function changeUserActiveState(accountId, userId, isActive) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ChangeActiveState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    userId: userId,
                    isActive: isActive
                }
            });
        }

        function changeUserOwnershipStatus(accountId, userId, isOwner) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ChangeOwnerStatus' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    userId: userId,
                    isOwner: isOwner
                }
            });
        }

        function sendPasswordLink(accountId, email) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/SendPasswordLink' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    email: email
                }
            });
        }


        /* ==========================================
          INVITATIONS
        ==========================================*/

        function getInvitations(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetInvitations/' + accountId + cacheBuster });
        }

        function inviteUser(accountId, email, firstName, lastName, role, isOwner) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/InviteUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    email: email,
                    firstName: firstName,
                    lastName: lastName,
                    role: role,
                    isOwner: isOwner,
                }
            });
        }

        function deleteInvitation(accountId, invitationKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteInvitation' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    invitationKey: invitationKey,
                }
            });
        }

        function resendInvitation(accountId, invitationKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResendInvitation' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    invitationKey: invitationKey,
                }
            });
        }


        /* ==========================================
          PASSWORD CLAIMS
        ==========================================*/

        function getPasswordClaims(accountId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetPasswordClaims/' + accountId + cacheBuster });
        }

        /* ==========================================
          ACCOUNT CLOSURE
        ==========================================*/

        function closeUnprovisionedAccount(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/CloseUnprovisionedAccount?accountId=' + accountId + cacheBuster });
        }

        function closeAccount(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/CloseAccount?accountId=' + accountId + cacheBuster });
        }

        function doesAccountRequireClosureApproval(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/DoesAccountRequireClosureApproval?accountId=' + accountId + cacheBuster });
        }

        function approveAccountClosure(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/ApproveAccountClosure?accountId=' + accountId + cacheBuster });
        }

        function reversAccounteClosureApproval(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/ReversAccounteClosureApproval?accountId=' + accountId + cacheBuster });
        }

        function reactivateSubscription(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/ReactivateSubscription?accountId=' + accountId + cacheBuster });
        }

        function accelerateAccountClosure(accountId) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/AccelerateAccountClosure?accountId=' + accountId + cacheBuster });
        }

        /* =========================================
           LOGS
        ===========================================*/

        function getAccountLog(accountId, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLog' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByCategory(accountId, categoryType, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByCategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    categoryType: categoryType,
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByActivity(accountId, activityType, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByActivity' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    activityType: activityType,
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByUser(accountId, userId, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    userId: userId,
                    maxRecords: maxRecords,
                }
            });
        }

        function getAccountLogByObject(accountId, objectId, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetAccountLogByObject' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    objectId: objectId,
                    maxRecords: maxRecords,
                }
            });
        }


        /* ==========================================
          LOG TYPES
        ==========================================*/

        function getAccountLogCategories() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountLogCategories' + cacheBuster });
        }

        function getAccountLogActivities() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountLogActivities' + cacheBuster });
        }

        /* ==========================================
          DATA INJECTION
        ==========================================*/

        function injectImageDocuments(accountId, imageDocumentCount) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/InjectImageDocuments?accountId=' + accountId + "&imageDocumentCount=" + imageDocumentCount + cacheBuster });

        }


        /* ==========================================
          HELPERS
        ==========================================*/

        var transformRequest = function (data, headersGetter) {
            var headers = headersGetter();
            headers['Authorization'] = 'WSSE profile="UsernameToken"';
            headers['X-WSSE'] = 'UsernameToken ' + nonce
            headers['Content-Type'] = 'application/json';
        };

    }
})();