(function () {
    'use strict';

    var controllerId = 'registrationController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'registrationServices',
            'registrationModels',
            '$location',
             registrationController
    ]);

    function registrationController(registrationServices, registrationModels, $location) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'registrationController';
        vm.activate = activate;

        /* ==========================================
               Helper Methods
        ==========================================*/

        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };


        //--------------------------------------------------------------------

        /* ==========================================
               VALIDATION STYLES
           ==========================================*/

        var inputDefaultStyle = { "border": "1px solid #6E6E6E" }
        var inputLoadingStyle = { "background-image": "url(\'/Images/Icons/loader.gif\')", "background-repeat": "no-repeat", "background-position": "right", "autocomplete": "off" };
        var inputSuccessStyle = { "background-image": "url(\'/Images/Icons/ok.png\')", "background-repeat": "no-repeat", "background-position": "right", "autocomplete": "off" };
        //var inputErrorStyle =   { "background-image": "url(\'/Images/Icons/error.png\')", "background-repeat": "no-repeat", "background-position": "right", "autocomplete": "off" };
        var inputErrorStyle = { "autocomplete": "off" };


        /* ==========================================
               VALIDATION DEFAULTS
           ==========================================*/

        vm.termsAgreedTo = false;

        vm.submissionProcessing = false;
        vm.submissionSuccess = false;
        vm.submissionMessage = "";
        vm.submissionMessage2 = "";

        vm.accountNameLabel = "Account Name";
        vm.accountNameStyle = inputDefaultStyle;

        vm.accountNameSubdomain = ""

        vm.firstNameLabel = "First Name";
        vm.firstNameStyle = inputDefaultStyle;

        vm.lastNameLabel = "Last Name";
        vm.lastNameStyle = inputDefaultStyle;

        vm.emailLabel = "Email";
        vm.emailStyle = inputDefaultStyle;

        vm.phoneNumberLabel = "Phone Number";
        vm.phoneNumberStyle = inputDefaultStyle;

        vm.passwordLabel = "Password";
        vm.passwordStyle = inputDefaultStyle;

        vm.confirmPasswordLabel = "Confirm Password";
        vm.confirmPasswordStyle = inputDefaultStyle;

        vm.passwordConfirmationAttempted = false;

        /* ==========================================
               VALIDATION METHODS
           ==========================================*/

        vm.validateAccountName = function () {
            vm.accountNameStyle = inputLoadingStyle;

            registrationServices.validateAccountName(vm.registrationEndpoint, vm.accountName)
                    .success(function (data) {

                        vm.accountNameValid = data.valid;
                        vm.accountNameLabel = data.message;

                        if (vm.accountNameValid) {
                            $("#subdomain").slideDown();

                            vm.accountNameStyle = inputSuccessStyle;
                            vm.accountNameSubdomain = data.subdomain;

                        }
                        else {
                            $("#subdomain").slideUp();
                            vm.accountNameStyle = inputErrorStyle;
                            vm.accountNameSubdomain = "";
                        }

                    })
                    .error(function (data) {

                        vm.accountNameStyle = inputErrorStyle;

                        vm.accountNameValid = false;
                        vm.accountNameLabel = "An error occurred. Please try again";
                        vm.accountNameSubdomain = data.subdomain;

                    });
        };

        vm.validateFirstName = function () {
            vm.firstNameStyle = inputLoadingStyle;

            registrationServices.validateFirstName(vm.registrationEndpoint, vm.firstName)
                    .success(function (data) {

                        vm.firstNameValid = data.valid;



                        if (vm.firstNameValid) {
                            vm.firstNameStyle = inputSuccessStyle;
                            vm.firstNameLabel = "First Name";

                        }
                        else {
                            vm.firstNameStyle = inputErrorStyle;
                            vm.firstNameLabel = data.message;
                        }

                    })
                    .error(function (data) {

                        vm.firstNameStyle = inputErrorStyle;

                        vm.firstNameValid = false;
                        vm.firstNameLabel = "An error occurred. Please try again";

                    });
        };

        vm.validateLastName = function () {
            vm.lastNameStyle = inputLoadingStyle;

            registrationServices.validateLastName(vm.registrationEndpoint, vm.lastName)
                    .success(function (data) {

                        vm.lastNameValid = data.valid;

                        if (vm.lastNameValid) {

                            vm.lastNameLabel = "Last Name";
                            vm.lastNameStyle = inputSuccessStyle;

                        }
                        else {
                            vm.lastNameStyle = inputErrorStyle;
                            vm.lastNameLabel = data.message;
                        }

                    })
                    .error(function (data) {

                        vm.lastNameStyle = inputErrorStyle;

                        vm.lastNameValid = false;
                        vm.lastNameLabel = "An error occurred. Please try again";

                    });
        };

        vm.validateEmail = function () {
            vm.emailStyle = inputLoadingStyle;

            registrationServices.validateEmail(vm.registrationEndpoint, vm.email)
                    .success(function (data) {

                        vm.emailValid = data.valid;

                        if (vm.emailValid) {
                            vm.emailStyle = inputSuccessStyle;
                            vm.emailLabel = "Email";

                        }
                        else {
                            vm.emailStyle = inputErrorStyle;
                            vm.emailLabel = data.message;
                        }

                    })
                    .error(function (data) {

                        vm.emailStyle = inputErrorStyle;

                        vm.emailValid = false;
                        vm.emailLabel = "An error occurred. Please try again";

                    });
        };

        vm.validatePhoneNumber = function () {
            vm.phoneNumberStyle = inputLoadingStyle;

            registrationServices.validatePhoneNumber(vm.registrationEndpoint, vm.phoneNumber)
                    .success(function (data) {

                        vm.phoneNumberValid = data.valid;

                        if (vm.phoneNumberValid) {
                            vm.phoneNumberStyle = inputSuccessStyle;
                            vm.phoneNumberLabel = "Phone Number";

                        }
                        else {
                            vm.phoneNumberStyle = inputErrorStyle;
                            vm.phoneNumberLabel = data.message;
                        }

                    })
                    .error(function (data) {

                        vm.phoneNumberStyle = inputErrorStyle;

                        vm.phoneNumberValid = false;
                        vm.phoneNumberLabel = "An error occurred. Please try again";

                    });
        };

        vm.validatePassword = function () {

            if (vm.passwordConfirmationAttempted) {
                vm.passwordConfirmationAttempted = false;
                vm.confirmPassword = "";
                vm.confirmPasswordValid = null;
                vm.confirmPasswordStyle = inputDefaultStyle;
                vm.confirmPasswordLabel = "Confirm Password";
            };
            vm.confirmPassword = "";

            vm.passwordStyle = inputLoadingStyle;

            if (vm.password.length >= vm.passwordMinLength) {
                vm.passwordValid = true;
                vm.passwordStyle = inputSuccessStyle;
                vm.passwordLabel = "Password";
            }
            else {
                vm.passwordStyle = inputErrorStyle;
                vm.passwordValid = false;
                vm.passwordLabel = "Password must be at least " + vm.passwordMinLength + " characters long";
            };
        };

        vm.validateConfirmPassword = function () {

            vm.passwordConfirmationAttempted = true;

            if (vm.confirmPassword == vm.password && vm.confirmPassword != "") {
                vm.confirmPasswordValid = true;
                vm.confirmPasswordStyle = inputSuccessStyle;
                vm.confirmPasswordLabel = "Confirm Password";
            }
            else {
                vm.confirmPasswordStyle = inputErrorStyle;
                vm.confirmPasswordValid = false;
                vm.confirmPasswordLabel = "Passwords do not match";
            };
        };

        /* ==========================================
               SUBMIT REGISTRATION
           ==========================================*/

        vm.submitRegistration = function () {


            vm.processingMessage = "Registering Your Account...";
            if (vm.planType == "demo") {
                vm.processingMessage = "Registering Demo Request...";
            }

            vm.submissionProcessing = true;


            registrationServices.submitRegistration(vm.registrationEndpoint, vm.accountName, vm.firstName, vm.lastName, vm.email, vm.phoneNumber, vm.password, vm.confirmPassword, vm.origin)
                    .success(function (data) {

                        vm.submissionProcessing = false;
                        vm.submissionSuccess = data.isSuccess;

                        if (vm.submissionSuccess) {
                            vm.submissionMessage = "Thank you for registering!";
                            vm.submissionMessage2 = "We will contact you as soon as possible.";
                        }
                        else {
                            vm.submissionMessage = data.ErrorMessage;
                        }
                    })
                    .error(function (data) {
                        vm.submissionProcessing = false;
                        vm.submissionSuccess = false;
                        vm.submissionMessage = "An error occurred. Please try again";

                    });
        };

        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        vm.planType = "";
        vm.srcType = "";
        vm.adType = "";

        activate();

        function activate() {

            vm.origin = "site";    

            try
            {
                vm.planType = $location.search().plan;
            }
            catch (e) {
            }
            try {
                vm.srcType = $location.search().src;
            }
            catch (e) {

            }
            try {
                vm.adType = $location.search().type;
            }
            catch (e) {

            }

            try {

                if (vm.planType != "" && vm.planType != undefined) {
                    vm.origin = "site | " + vm.planType;
                }
                else {
                    vm.origin = "site";
                }

                if (vm.adType != "" && vm.adType != undefined)
                {
                    vm.origin += " | " + vm.adType;
                }
                if (vm.srcType != "" && vm.srcType != undefined) {
                    vm.origin += " | " + vm.srcType;
                }
            }
            catch (e) {
                vm.origin = "site";
            }



            //console.log(vm.origin);
            //console.log("[" + vm.planType + "]");

            // Injected variables from the view (via CoreServices/PlatformSettings)
            vm.registrationEndpoint = registrationEndpoint; //<-- passed in from CoreServices. Differs based on environment. Must pass into Services.
            vm.passwordMinLength = passwordMinLength;
            vm.trialLength = trialLength;
            vm.accountPortalUrl = accountPortalUrl;

            vm.termsLink = termsLink;
            vm.privacyLink = privacyLink;
            vm.acceptableUseLink = acceptableUseLink;
            vm.serviceAgreement = serviceAgreement;
            vm.theCurrentYear = new Date().getFullYear();

            //$("footerDiv").show();
            //var footer = document.getElementById("footerDiv");
            //footer.show();
            //Debug.trace('registration controller activation complete');

        };

    };

})();

