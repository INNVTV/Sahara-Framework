(function () {
    'use strict';

    var controllerId = 'marketplaceIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'commerceServices',
            'accountServices',
            'sharedServices',
            'marketplaceIndexModels',

             marketplaceIndexController
    ]);

    function marketplaceIndexController(commerceServices, accountServices, marketplaceIndexModels, sharedServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'marketplaceIndexController';
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


        //Account User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var userRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)




















        /* ============================================================

            Credit Card Management

        ================================================================*/

        //Models, etc...
        vm.creditCardInfo = null;
        //vm.hasCreditCard = true;

        //GET CARD DATA -------------------------------------------
        vm.getCreditCard = function () {

            //Debug.trace("method called...");

            //vm.creditCardLoading = true;

            accountServices.getCardInfo()
                .success(function (data, status, headers, config) {
                    
                    if (data.CardDescription != null)
                    {
                        vm.creditCardInfo = data;
                    }
                               

                })
                .error(function (data, status, headers, config) {

                })
        }



        /* ----- Adding/Updating Card -----------------*/


        vm.actvateNewCreditCardModal = function () {
            vm.newCard.reset();
        }


        vm.newCard = {

            name: null,
            number: null,
            cvc: null,
            expMonth: null,
            expYear: null,

            error: false,
            errorMessage: null,
            errorMessages: [],

            isSuccess: false,
            processing: false,

            reset: function () {
                this.name = null;
                this.number = null;
                this.cvc = null;
                this.expMonth = null;
                this.expYear = null;

                this.error = false;
                this.errorMessage = null;
                this.errorMessages = [];

                this.isSuccess = false;
                this.processing = false;
            },

        }

        vm.submitNewCreditCard = function () {
            vm.newCard.processing = true;

            vm.newCard.isSuccess = false;
            vm.newCard.error = false;
            vm.newCard.errorMessage = null;
            vm.newCard.errorMessages = [];

            accountServices.addUpdateCard(vm.newCard.name, vm.newCard.number, vm.newCard.cvc, vm.newCard.expMonth, vm.newCard.expYear)
                .success(function (data, status, headers, config) {

                    vm.newCard.processing = false;

                    if (data.isSuccess) {
                        vm.newCard.isSuccess = true;
                        vm.getCreditCard();
                    }
                    else {
                        vm.newCard.error = true;
                        vm.newCard.errorMessage = data.ErrorMessage;
                        vm.newCard.errorMessages = data.ErrorMessages;
                    }

                }).error(function (data, status, headers, config) {

                    vm.newCard.processing = false;

                    vm.newCard.error = true;
                    vm.newCard.errorMessage = "There was an issue contacting the service, please try again in a moment.";
                })

        }














        /* ==========================================================

           CREDITS

        ===========================================================*/

        /* Variables ---------------------*/

        vm.creditsToDollarExchangeRate = null;
        vm.availableCredits = 0;

        vm.creditsTransactionLog = null;

        vm.creditsToPurchaseAmount = 25;
        vm.creditsToSpendAmount = 5;
        vm.spendCreditsDescription = "Imaging services";
        vm.showCreditsDescription = true;

        vm.creditsToTradeAmount = 5;
        vm.creditsToTradeAccountName = null;
        vm.creditsToTradeAccountID = null;
        vm.tradeCreditsDescription = "Assistance with a task";
        vm.showTradeDescription = true;
        vm.showTradeAccount = true;

        vm.buyCreditsProcessing = false;
        vm.buyCreditsResponse = null;
        vm.buyCreditsProcessingMessage = null;
        

        vm.spendCreditsProcessing = false;
        vm.spendCreditsResponse = null;
        vm.spendCreditsProcessingMessage = null;

        vm.tradeCreditsProcessing = false;
        vm.tradeCreditsResponse = null;
        vm.tradeCreditsProcessingMessage = null;
        

        vm.updateCreditsForPurchase = function(amount)
        {
            vm.creditsToPurchaseAmount = vm.creditsToPurchaseAmount + amount;
        }

        vm.updateCreditsForSpending = function (amount) {
            vm.creditsToSpendAmount = vm.creditsToSpendAmount + amount;
        }

        vm.updateCreditsForTrade = function (amount) {
            vm.creditsToTradeAmount = vm.creditsToTradeAmount + amount;
        }

        vm.editCreditsDescription = function ()
        {
            vm.showCreditsDescription = false;
            vm.spendCreditsEditDescription = vm.spendCreditsDescription;
        }
        vm.cancelCreditsDescription = function() {
            vm.showCreditsDescription = true;
            vm.spendCreditsDescription = vm.spendCreditsEditDescription;
        }

        vm.editTradeAccount = function () {
            vm.showTradeAccount = false;
        }
        vm.cancelTradeAccount = function () {
            vm.showTradeAccount = true;
        }

        vm.editTradeDescription = function () {
            vm.showTradeDescription = false;
            vm.tradeCreditsEditDescription = vm.tradeCreditsDescription;
        }
        vm.cancelTradeDescription = function () {
            vm.showTradeDescription = true;
            vm.tradeCreditsDescription = vm.tradeCreditsEditDescription;
        }
        vm.updateTradeAccount = function (account) {
            vm.creditsToTradeAccountName = account.AccountName;
            vm.creditsToTradeAccountID = account.AccountID;
        }

        /* Settings (managed by settings panel) -------
        vm.creditPurchaseDollarAmount = 5;
        vm.creditPaymentAmount = 25;
        vm.creditPaymentDescription = "Imaging service";*/


        vm.loadCreditsPanel = function()
        {
            vm.getExchangeRate();
            vm.getAvailableCredits();
            vm.getAccountsList();
            vm.getCreditsTransactionLog();
        }


        /* ==========================================
           Credit Methods
        ==========================================*/

        // Get Credits Methods ---------------------------

        vm.getExchangeRate = function () {

            //Debug.trace("Getting exchange rate...");

            commerceServices.getCreditsToDollarExchangeValue()
            .success(function (data, status, headers, config) {


                vm.creditsToDollarExchangeRate = eval(data);

                //Debug.trace("Data:" + vm.creditsToDollarExchangeRate  + data);

            })
            .error(function (data, status, headers, config) {

            })
        }

        vm.getAvailableCredits = function () {

            //Debug.trace("Getting available credits...");

            commerceServices.getAvailableCredits()
            .success(function (data, status, headers, config) {


                var countUpOptions = {
                    useEasing: true,
                    useGrouping: true,
                    separator: ',',
                    decimal: '.',
                    prefix: '',
                    suffix: '',
                }

                var previousCredits = 0;

                try {
                    previousCredits = vm.availableCredits;
                }
                catch (err) {
                }

                try {
                    var countUpExecution = new countUp("countUpElement", previousCredits, eval(data), 0, 1.5, countUpOptions);
                    countUpExecution.start();
                    vm.availableCredits = eval(data);
                }
                catch (err) {
                    vm.availableCredits = eval(data);
                }

                vm.availableCredits = eval(data);

            })
            .error(function (data, status, headers, config) {

            })
        }


        // Buy Credits Methods ---------------------------

        vm.buyCredits = function () {

            var dollarAmount = (vm.creditsToPurchaseAmount / vm.creditsToDollarExchangeRate) + '.00';

            vm.buyCreditsResponse = null;
            vm.buyCreditsProcessing = true;
            //$("#buyCreditsResponse").slideUp();
            //Debug.trace("buying $" + dollarAmount + " in credits");

            commerceServices.buyCredits(dollarAmount)
            .success(function (data, status, headers, config) {

                //$("#buyCreditsResponse").slideDown(60);

                vm.buyCreditsResponse = data;
                vm.buyCreditsProcessing = false;
                
                if (data.isSuccess) {
                    
                    vm.buyCreditsResponse.SuccessMessage = (dollarAmount * vm.creditsToDollarExchangeRate) + " credits have been purchased! Adding to your account..."

                    //Refresh transaction log
                    vm.getCreditsTransactionLog();
                }
                else {
                    vm.buyCreditsResponse.ErrorMessage = data.ErrorMessage;
                }

               // $("#buyCreditsResponse").fadeIn(1600);
                //$("#buyCreditsResponse").slideDown(800);
                $('#buyCreditsResponse').stop(true, true).fadeIn(300).css('display', 'none').slideDown(600);
                $('#buyCreditsButtonResponse').fadeIn(300);

                setTimeout(function () {
                    //Update available credits:
                    vm.getAvailableCredits();
                }, 920);
                setTimeout(function () {
                    $("#buyCreditsResponse").fadeOut(600);
                }, 3400);

                setTimeout(function () {
                    $("#buyCreditsButtonResponse").fadeOut(300);
                }, 2800);

            })
            .error(function (data, status, headers, config) {
                
            })
        }

        // Spend Credits Methods ---------------------------

        vm.spendCredits = function () {


            vm.spendCreditsResponse = null;
            vm.spendCreditsProcessing = true;

            //Debug.trace("spending" + vm.creditsToSpendAmount + " credits");

            commerceServices.spendCredits(vm.creditsToSpendAmount, vm.spendCreditsDescription)
            .success(function (data, status, headers, config) {


                vm.spendCreditsResponse = data;
                vm.spendCreditsProcessing = false;

                if (data.isSuccess) {

                    vm.spendCreditsResponse.SuccessMessage = vm.creditsToSpendAmount + " credits have been spent. Deducting from your account..."

                    //Refresh transaction log
                    vm.getCreditsTransactionLog();
                }
                else {
                    vm.spendCreditsResponse.ErrorMessage = data.ErrorMessage;
                }

                $('#spendCreditsResponse').stop(true, true).fadeIn(300).css('display', 'none').slideDown(600);
                $('#spendCreditsButtonResponse').fadeIn(300);

                setTimeout(function () {
                    //Update available credits:
                    vm.getAvailableCredits();
                }, 920);
                setTimeout(function () {
                    $("#spendCreditsResponse").fadeOut(600);
                }, 3400);

                setTimeout(function () {
                    $("#spendCreditsButtonResponse").fadeOut(300);
                }, 2800);

            })
            .error(function (data, status, headers, config) {

            })
        }



        // Trade Credits Methods ---------------------------

        vm.tradeCredits = function () {


            vm.tradeCreditsResponse = null;
            vm.tradeCreditsProcessing = true;

            //Debug.trace("trading" + vm.creditsToTradeAmount + " credits");

            commerceServices.tradeCredits(vm.creditsToTradeAccountID, vm.creditsToTradeAmount, vm.tradeCreditsDescription)
            .success(function (data, status, headers, config) {


                vm.tradeCreditsResponse = data;
                vm.tradeCreditsProcessing = false;

                if (data.isSuccess) {

                    vm.tradeCreditsResponse.SuccessMessage = vm.creditsToTradeAmount + " credits have been traded. Deducting from your account..."

                    //Refresh transaction log
                    vm.getCreditsTransactionLog();
                }
                else {
                    vm.tradeCreditsResponse.ErrorMessage = data.ErrorMessage;
                }

                $('#tradeCreditsResponse').stop(true, true).fadeIn(300).css('display', 'none').slideDown(600);
                $('#tradeCreditsButtonResponse').fadeIn(300);

                setTimeout(function () {
                    //Update available credits:
                    vm.getAvailableCredits();
                }, 920);
                setTimeout(function () {
                    $("#tradeCreditsResponse").fadeOut(600);
                }, 3400);

                setTimeout(function () {
                    $("#tradeCreditsButtonResponse").fadeOut(300);
                }, 2800);

            })
            .error(function (data, status, headers, config) {

            })
        }


        // Get Accounts List -------------------------

        vm.getAccountsList = function () {
            //Debug.trace("Getting accounts list...");

            setTimeout(function () {

                accountServices.getAccountsList()
                .success(function (data, status, headers, config) {

                    vm.accountsList = data;

                    vm.creditsToTradeAccountName = vm.accountsList[0].AccountName;
                    vm.creditsToTradeAccountID = vm.accountsList[0].AccountID;

                })
                .error(function (data, status, headers, config) {

                })


            }, 1000);


        }


        // Credits Transaction Log -------------------------

        vm.getCreditsTransactionLog = function()
        {
            //Debug.trace("Getting credits transaction log...");

            setTimeout(function () {


                accountServices.getAccountLogByCategory("Credits", 15)
                .success(function (data, status, headers, config) {

                    vm.creditsTransactionLog = data;
                })
                .error(function (data, status, headers, config) {

                })


            }, 2000);

            
        }



































        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.userRoles.indexOf(data.Role) //<-- use ACCOUNT roles, NOT PLATFORM roles!

                if (vm.currentUserProfile.Id == "" || vm.currentUserProfile == null)
                {
                    //Log user out if empty
                    window.location.replace("/login");
                }

                //Debug.trace("Profile refreshed!");
                //Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }

        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.userRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

            if (vm.currentUserProfile.Id == "") {
                //Log user out if empty
                window.location.replace("/login");
            }

            //Update user profile info in case of role updates
            updateCurrentUserProfile();
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 320000);


            //Load credits panel data
            vm.loadCreditsPanel();

            //Load credit card data
            vm.getCreditCard();

            //Debug.trace('marketplaceIndexController activation complete');



            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT Platform roles!

                //Debug.trace("Lowest role allowed: '" + lowestRoleAllowed + "'");
                //Debug.trace("Comparing: User: '" + currentUserRoleIndex + "' Allowed: '" + allowedIndex + "'");

                if (currentUserRoleIndex >= allowedIndex) {
                    //Debug.trace("Allowed!");
                    return true;
                }
                else {
                    //Debug.trace("Not allowed!");
                    return false;
                }
            }
        }

    }

})();

