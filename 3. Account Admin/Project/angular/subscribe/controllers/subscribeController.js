(function () {
    'use strict';

    var controllerId = 'subscribeController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'sharedServices',
            'accountServices',
            'paymentPlanServices',
            subscribeController,
    ]);

    function subscribeController(sharedServices, accountServices, paymentPlanServices) {


        

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'subscribeController';
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




        /* ==========================================
                Activation methods
        ==========================================*/

        vm.showLoader = true;


        //Get Account
        vm.getAccount = function () {

            accountServices.getAccountByIdForSub(vm.accountId)
                    .success(function (data, status, headers, config) {
                        vm.account = data;
                        vm.getPlan();
                    })
                    .error(function (data, status, headers, config) {

                    })
        }


        //Get Plan
        vm.getPlan = function () {

            paymentPlanServices.getPlan(vm.planName)
                    .success(function (data, status, headers, config) {
                        vm.plan = data;

                        if (vm.frequencyMonths == '1' || vm.frequencyMonths == '12')
                        {
                            //We only allow monthly (1) or yearly (12) otherwise disable loading
                            vm.showLoader = false;
                        }
                        else {
                            vm.showLoader = false;
                            vm.frequencyMonths = "";
                        }
                        
                    })
                    .error(function (data, status, headers, config) {

                    })
        }




        /* ==========================================
               Subscribe Methods
        ==========================================*/

        vm.processingSubscription = false;
        vm.subscriptionComplete = false;


        vm.subscriptionCard = {

            name: null,
            number: null,
            cvc: null,
            expMonth: null,
            expYear: null,

            error: false,
            errorMessage: null,

            reset: function () {
                this.name = null;
                this.number = null;
                this.cvc = null;
                this.expMonth = null;
                this.expYear = null;

                this.error = false;
                this.errorMessage = null;
            },

        }

        vm.processSubscription = function()
        {
            vm.processingSubscription = true;
            
            paymentPlanServices.subscribeFromSub(vm.account.AccountID, vm.plan.PaymentPlanName, vm.frequencyMonths, vm.subscriptionCard.name, vm.subscriptionCard.number, vm.subscriptionCard.cvc, vm.subscriptionCard.expMonth, vm.subscriptionCard.expYear)
                .success(function (data, status, headers, config) {

                    vm.processingSubscription = false;

                    if (data.isSuccess) {
                        vm.subscriptionComplete = true;
                    }
                    else {
                        vm.subscriptionCard.error = true;
                        vm.subscriptionCard.errorMessage = data.ErrorMessage;
                    }

                })
                    .error(function (data, status, headers, config) {
                        vm.subscriptionCard.error = true;
                        vm.subscriptionCard.errorMessage = "There was an issue contacting the service, please try again in a moment.";
                        vm.processingSubscription = false;
                    })
        }






        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)

            // Load subscription info for the platfor user.
            vm.planName = JSON.parse(planName);
            vm.frequencyMonths = JSON.parse(frequencyMonths);
            vm.accountId = JSON.parse(accountId);


            vm.termsLink = termsLink;
            vm.privacyLink = privacyLink;
            vm.acceptableUseLink = acceptableUseLink;
            vm.serviceAgreement = serviceAgreement;
            vm.theCurrentYear = new Date().getFullYear();

            vm.getAccount();


            //Debug.trace('subscribeController activation complete');

        }

    }

})();

