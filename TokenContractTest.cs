using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Contracts.TokenConverter;
using AElf.CSharp.Core;
using AElf.Types;
using AElfChain.Common;
using AElfChain.Common.Contracts;
using AElfChain.Common.DtoExtension;
using AElfChain.Common.Helpers;
using AElfChain.Common.Managers;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using log4net;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Xunit;
using Xunit.Sdk;
using Type = System.Type;

namespace AElf.Automation.Contracts.ScenarioTest
{
    [TestClass]
    public class TokenContractTest
    {
        private readonly List<string> ResourceSymbol = new List<string>
            {"CPU", "NET", "DISK", "RAM", "READ", "WRITE", "STORAGE", "TRAFFIC","ELF","SHARE"};

        private TokenContractContainer.TokenContractStub _bpTokenSub;
        private GenesisContract _genesisContract;
        private ParliamentContract _parliamentContract;
        private TokenConverterContractContainer.TokenConverterContractStub _testTokenConverterSub;
        private TokenContractContainer.TokenContractStub _testTokenSub;

        private TokenContract _tokenContract;
        private TokenConverterContract _tokenConverterContract;
        private TokenConverterContractContainer.TokenConverterContractStub _tokenConverterSub;
        private TokenContractContainer.TokenContractStub _tokenSub;
        private  ILog Logger { get; set; }
        private INodeManager NodeManager { get; set; }
        private AuthorityManager AuthorityManager { get; set; }

        private string InitAccount { get; } = "SxKLxnwXLfF9gF4dRbHE8jEXpHQFWVQ1XCeqrAahDMHQTmcJU";
        private string BpAccount { get; } = "2ZYyxEH6j8zAyJjef6Spa99Jx2zf5GbFktyAQEBPWLCvuSAn8D";
        private string TestAccount { get; } = "YF8o6ytMB7n5VF9d1RDioDXqyQ9EQjkFK3AwLPCH2b9LxdTEq";
        private string Account { get; } = "2a6MGBRVLPsy6pu4SVMWdQqHS5wvmkZv8oas9srGWHJk7GSJPV";
        
        private static string RpcUrl { get; } = "127.0.0.1:8000";
        private string Symbol { get; } = "TEST";
        private string Symbol1 { get; } = "NOPROFIT";
        private string Symbol2 { get; } = "NOWHITE";
        private string Symbol3 { get; } = "ADF";
        

        [TestInitialize]
        public void Initialize()
        {
            Log4NetHelper.LogInit("TokenContractTest");//
            Logger = Log4NetHelper.GetLogger();//
            NodeInfoHelper.SetConfig("nodes.json");

            NodeManager = new NodeManager(RpcUrl);//
            AuthorityManager = new AuthorityManager(NodeManager, InitAccount);//
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, InitAccount);
            _tokenContract = _genesisContract.GetTokenContract(InitAccount);
            _parliamentContract = _genesisContract.GetParliamentContract(InitAccount);
            _tokenConverterContract = _genesisContract.GetTokenConverterContract(InitAccount);

            _tokenSub = _genesisContract.GetTokenStub(InitAccount);
            _bpTokenSub = _genesisContract.GetTokenStub(BpAccount);
            _testTokenSub = _genesisContract.GetTokenStub(TestAccount);

            _tokenConverterSub = _genesisContract.GetTokenConverterStub(InitAccount);
            _testTokenConverterSub = _genesisContract.GetTokenConverterStub(TestAccount);
        }
        
        /****** Passed Test Cases***************************/
        
        [TestMethod]
        public void Create_Success1_Test()
        {
            var symbol = "ADF";
            var chainId = NodeManager.GetChainId();
            //chainId="AElf"
            //NodeManager returned = NodeManager
            //NodeManager.GetChainId() returned = "AELF"
            //var intChainId = ChainHelper.ConvertBase58ToChainId(".");//test Base58

            var intChainId = ChainHelper.ConvertBase58ToChainId(chainId);
            var createInput1 = new CreateInput()
            {
                Symbol="ADF",
                TokenName = "TokenName1",
                TotalSupply= 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
            };
            
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput1);
            var tokenInfo =  _tokenContract.GetTokenInfo(symbol);
            symbol.ShouldBe(tokenInfo.Symbol);
            createInput1.TokenName.ShouldBe(tokenInfo.TokenName);
        }

        [TestMethod]
        public void Create_FailedWithTokenAlreadyExists_Test()
        {
            var symbol = "AMC";
            var chainId = NodeManager.GetChainId();
            var intChainId = ChainHelper.ConvertBase58ToChainId(chainId);
            var createInput1 = new CreateInput()
            {
                Symbol="AA",
                TokenName = "TokenName2",
                TotalSupply= 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
                IssueChainId = intChainId
            };
            
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput1);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            
            var tokenInfo =  _tokenContract.GetTokenInfo(symbol);
            symbol.ShouldBe(tokenInfo.Symbol);
            createInput1.TokenName.ShouldBe(tokenInfo.TokenName);
           // result.Error.ShouldBe("AElf.Sdk.CSharp.AssertionException: Token already exists.");//Run Success
            result.Error.ShouldContain("Token already exists.");//Run Success
            var error = result.Error;
            //error.ShouldBeSameAs("AElf.Sdk.CSharp.AssertionException: Token already exists.");//Run Failed
            
            /*result.Error.ShouldBe("Token already exists.");
             result.Error
            should be
            "Token already exists."
            but was
            "AElf.Sdk.CSharp.AssertionException: Token already exists."
            difference*/
        }
        
        [TestMethod]
        public void Create_Success3_Test()
        { 
            /*EventHandler<Guid> eventAge=(obj,ages)=>{};
            eventAge?.Invoke(this,Guid.Empty);*/
            
            var symbol = "AZA"; //Need to change the symbol name when u run the test case next time. Because after first run, the symbol has already been created, u can not create A token with the same symbol.
            var chainId = NodeManager.GetChainId();
            var intChainId = ChainHelper.ConvertBase58ToChainId(chainId);
            var createInput1 = new CreateInput()
            {
                Symbol=symbol,
                TokenName = "TokenName4",
                TotalSupply= 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
                IssueChainId = intChainId
            };
            
            //result can get created toke info, u can valid the state of the created token, it will prompt an error message when create token failed
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput1);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            //result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            
            //after create token successful, u can get the NonIndexed value of first log with name TokenCreated in Logs
            var logEvent = result.Logs.First(l => l.Name.Equals(nameof(TokenCreated))).NonIndexed;
            var created = TokenCreated.Parser.ParseFrom(ByteString.FromBase64(logEvent));
            created.Decimals.ShouldBe(createInput1.Decimals);
            Console.WriteLine(logEvent);
            
            var tokenInfo =  _tokenContract.GetTokenInfo(symbol);
            symbol.ShouldBe(tokenInfo.Symbol);
            createInput1.TokenName.ShouldBe(tokenInfo.TokenName);
        }

        [TestMethod]
        public void GetChainId()
        {
            var chainId = NodeManager.GetChainId();
            var intChainId = ChainHelper.ConvertBase58ToChainId(chainId);
            Logger.Info($"{chainId} ==> {intChainId}"); //AELF ==> 9992731
        }
        
        [TestMethod]
        [DataTestMethod]
        [DataRow("ABCDEFGHIGKLMN",18,"TokenName")]
        [DataRow("ABC",20,"aaa")]
        [DataRow("",18,"bbb")]
        public void Create_InvalidInput_ReturnInvalidInputMessage(
            string symbol, int decimals, string tokenName)
        {
            /* public const int TokenNameLength = 80;
              public const int MaxDecimals = 18;
              public const int SymbolMaxLength = 10;
              public const int MemoMaxLength = 64; */
            
            var createInput7 = new CreateInput()
            {
                Symbol=symbol,
                TokenName = tokenName,
                TotalSupply= 111,
                Decimals = decimals,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
            };
        
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput7);
            Console.WriteLine(result);//Details are not available
            //Stacktrace is not available
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            result.Error.ShouldContain("Invalid input.");
            
        }
        
        [TestMethod]
        [DataTestMethod]
        [DataRow("ABCDEFGHIGKLMN",18,"mysfuhpwqrxiqrexmsplvrgmueahggsxgkawcveiavpgwglmebdcrqlvrgkpghqkzacfqivqnhkzbapotodfcpfpyfevlpbtffgw")]
        [DataRow("ABC",20,"aaa")]
        [DataRow("ABCAAAAAAAAAAA",20,"aaa")]
        public void Create_InvalidInputWithRandomTokenName_ReturnInvalidInputMessage(
            string symbol, int decimals, string tokenName)
        {
            var createInput7 = new CreateInput() 
            {
                Symbol=symbol,
                TokenName = tokenName,
                TotalSupply= 111,
                Decimals = decimals,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput7);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            result.Error.ShouldContain("Invalid input.");
            Logger.Info(result);
        }

        [TestMethod]
        public void GetNativeTokenSymbol()
        {
            var token = _tokenContract.GetNativeTokenSymbol();
            Console.WriteLine(token);//ELF
        }
        
        [TestMethod]
        public void Create_RegisterTokenIsNativeToken_ReturnErrorMessage()
        {
            var createInput6 = new CreateInput()
            {
                Symbol="ELF",
                TokenName = "TokenName4",
                TotalSupply= 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
            };
            var token = _tokenContract.GetNativeTokenSymbol();//ELF
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput6);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            result.Error.ShouldContain("Token already exists.");
            Logger.Info(result);
        }
        
        [TestMethod]
        [DataTestMethod]
        [DataRow("CC","",1000,"aaa")]
        [DataRow("CC","QQQ",-1,"aaa")]
        [DataRow("CC","QQQ",1000,null)]
        [DataRow("ELF","QQQ",1000,null)]//ELF is NativeTokenSymbol
        public void Create_RegisterTokenWithInvalidInfo_ReturnErrorMessage(
            string symbol, string tokenName,int totalSupply,string issuer)
        {
            var createInput8 = new CreateInput()
            {
                /*
                 symbol_ = other.symbol_;
                tokenName_ = other.tokenName_;
                totalSupply_ = other.totalSupply_;
                decimals_ = other.decimals_;
                issuer_ = other.issuer_ != null ? other.issuer_.Clone() : null;
                isBurnable_ = other.isBurnable_;
                lockWhiteList_ = other.lockWhiteList_.Clone();
                issueChainId_ = other.issueChainId_;
                _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
                */
                
                Symbol=symbol,
                TokenName = tokenName,
                TotalSupply= totalSupply,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput8);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            result.Error.ShouldContain("Token already exists.");
            Logger.Info(result);
        }

        [TestMethod]
        public void GetSystemContractAddress()
        {
            var address = _parliamentContract.Contract;
            Console.WriteLine(address);//"2JT8xzjR5zJ8xnBvdgBZdSjfbokFSbF5hDdpUCbXeWaJfPDmsK"
            var address2 = _parliamentContract.ContractAddress;
            Console.WriteLine(address2);//2JT8xzjR5zJ8xnBvdgBZdSjfbokFSbF5hDdpUCbXeWaJfPDmsK
            
            /*var crossChainTransferAddress = _tokenSub.GetCrossChainTransferTokenContractAddress;
            Console.WriteLine(crossChainTransferAddress);
            */
        }
        
        [TestMethod]
        public void Create_WithSystemContractAddress1_Success()
        {
            var createInput5 = new CreateInput()
            {
                Symbol = "SD",
                TokenName = "TokenName11",
                TotalSupply = 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
                LockWhiteList =
                {
                    _tokenContract.Contract
                }
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput5);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            //create new token successful with SystemContractAddress, ie. WithAddressInLockWhiteList
            //the AddressInLockWhiteList means that the address is SystemContractAddress.
        }
        
        [TestMethod]
        public async Task Create_WithSystemContractAddress2_Success()
        {
            /*
             * if the address of the new token is not the system contract address,
             * it should show the message:
             * "Addresses in lock white list should be system contract addresses"
             *
             * if the address of the new token is the system contract address,
             * then set the token state to be true in LockWhiteList,
             * State.LockWhiteLists[input.Symbol][address] = true;
             * it means that after create success, the token's isWhiteList.Value should be true.
             */
            var createInput5 = new CreateInput()
            {
                Symbol = "SD",
                TokenName = "TokenName11",
                TotalSupply = 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
                LockWhiteList =
                {
                    _parliamentContract.Contract
                }
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput5);
            var isWhiteList = await _tokenSub.IsInWhiteList.CallAsync(new IsInWhiteListInput
            {
                Symbol = "SD",
                //Address = Address.FromBase58("2JT8xzjR5zJ8xnBvdgBZdSjfbokFSbF5hDdpUCbXeWaJfPDmsK")
                Address= _parliamentContract.Contract
            });
            isWhiteList.Value.ShouldBeTrue();
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
        }

        [TestMethod]
        public async Task Create_WithSystemContractAddress3_Success()
        {
            var createInput5 = new CreateInput()
            {
                Symbol = "SD",
                TokenName = "TokenName11",
                TotalSupply = 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
                LockWhiteList =
                {
                    _tokenContract.Contract
                }
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput5);
            var isWhiteList = await _tokenSub.IsInWhiteList.CallAsync(new IsInWhiteListInput
            {
                Symbol = "SD",
                Address = _parliamentContract.Contract
            });
            isWhiteList.Value.ShouldBeTrue();
            //create new token successful with SystemContractAddress, ie. WithAddressInLockWhiteList
            //the AddressInLockWhiteList means that the address is SystemContractAddress.
        }

        [TestMethod]
        public async Task Create_IsSystemContract_Test()
        {
            var isWhiteList = await _tokenSub.IsInWhiteList.CallAsync(new IsInWhiteListInput
            {
                //_bpTokenSub.IsInWhiteList;
                Symbol = "SD",
                Address = Address.FromBase58("2JT8xzjR5zJ8xnBvdgBZdSjfbokFSbF5hDdpUCbXeWaJfPDmsK")
            });
            isWhiteList.Value.ShouldBeTrue();
        }

        /****** Debugging Cases Separation Line ****************************/
        
        [TestMethod]
        public void Create_FailedWhenSideChainCreatorAlreadySet_ReturnMessage()
        {
            var symbol = "AMC";
            var createInput4 = new CreateInput()
            {
                Symbol=symbol,
                TokenName = "TokenName2",
                TotalSupply= 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput4);
            result.Error.ShouldBe("Failed to create token if side chain creator already set.");
        }
        
        [TestMethod]
        [DataTestMethod()]
        [TestProperty("input","")]
        [TestProperty("input","")]
        public void Create_InvalidCreate_ReturnInvalidInput()
        {
            var createInput1 = new CreateInput()
            {
                Symbol="Q",
                TokenName = "TokenName4",
                TotalSupply= 111,
                Decimals = 1,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = true,
            };
        }
        
        /*
         * /Users/duansale/Documents/github/aelf-automation-test/src/AElfChain.Common/Protobuf/Proto/acs0.proto
         * /Users/duansale/Documents/github/aelf-automation-test/src/AElfChain.Common/Protobuf/Generated/TokenContract.c.cs
         * /Users/duansale/Documents/github/aelf-automation-test/src/AElfChain.Common/Contracts/GensisContractExtension.cs
         * /Users/duansale/Documents/github/aelf-automation-test/test/AElf.Automation.FeatureVerification/bin/Debug/netcoreapp3.1/logs/TokenContractTest_2021-06-02.log
         * log the automation test result in Rider
         *
         * Transaction 436e8bc6f4c6a4928284c05783b89fbc4f1eaa1da5b60c64dfa424dbb22f6dbc
         * after create a new token successful, u can check the token info log in SwaggerUI with the Transaction (ie.the fire log)
         *
         * Context.LogDebug(() => $"Token created: {input.Symbol}");
         * Log the created token info in the dotnet iTerm, u can search by keys Token created.
         *
         */
        
        /****** SetPrimaryTokenSymbol Test *************************************/

        [TestMethod]
        public async Task SetPrimaryTokenSymbol_Test()
        {
            //var symbol = _tokenContract.GetPrimaryTokenSymbol();//ELF
            var other = new SetPrimaryTokenSymbolInput()
            {
                Symbol = "EEE"
            };
            var result = await _tokenSub.SetPrimaryTokenSymbol.CallAsync(new SetPrimaryTokenSymbolInput(other)
            {
                Symbol = other.Symbol
            });
        }

        
        /****** Issue Test *************************************/

        [TestMethod]
        public void Issue_Success_Test()
        {
            var issueInput1 = new IssueInput()
            {
                Amount = 1,
                Memo = "",
                Symbol = "ADF",
                To = Address.FromBase58(TestAccount),
            };

            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput1);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var tokenInfo = _tokenContract.GetTokenInfo("ADF");
            tokenInfo.Issued.ShouldBe(16);
            tokenInfo.Supply.ShouldBe(16);
            result.Error.ShouldContain("Invalid amount.");
            //MethodName: Issue, Parameter: { "symbol": "ADF", "to": "YF8o6ytMB7n5VF9d1RDioDXqyQ9EQjkFK3AwLPCH2b9LxdTEq" }
            //Error Message: AElf.Sdk.CSharp.AssertionException: Invalid amount.
            //Console.WriteLine(tokenInfo);
            //{ "symbol": "ADF", "tokenName": "TokenName1", "supply": "16", "totalSupply": "111", "decimals": 1, "issuer": "SxKLxnwXLfF9gF4dRbHE8jEXpHQFWVQ1XCeqrAahDMHQTmcJU", "isBurnable": true, "issueChainId": 9992731, "issued": "16" }
        }
        
        [TestMethod]
        public void Issue_InvalidAmount_Test()
        {
            var issueInput1 = new IssueInput()
            {
                Amount = 0,
                Memo = "",
                Symbol = "ADF",
                To = Address.FromBase58(TestAccount),
            };

            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput1);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            result.Error.ShouldContain("Invalid amount.");
        }
        
        [TestMethod]
        public void Issue_ToAddressNotFill_Test()
        {
            var issueInput1 = new IssueInput()
            {
                Amount = 110,
                Memo = "",
                Symbol = "ADF",
                To = null,
            };

            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput1);
            result.Error.ShouldContain("To address not filled.");
        }

        [TestMethod]
        public void Issue_TotalSupplyExceeded_Test()
        {
            var issueInput1 = new IssueInput()
            {
                Amount = 112,
                Memo = "",
                Symbol = "ADF",
                To = Address.FromBase58(InitAccount),
            };

            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput1);
            result.Error.ShouldContain("Total supply exceeded");
        }

        [TestMethod]
        public void Issue_Balance1_Test()
        {
            var symbol = "ADF";
            var amount = 1;
            var balance = _tokenContract.GetUserBalance(TestAccount, symbol);
            var tokenInfo = _tokenContract.GetTokenInfo(symbol);
            var issueInput3 = new IssueInput()
            {
                Amount = amount,
                Symbol = symbol,
                To = Address.FromBase58(TestAccount),
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput3);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            
            var afterBalance = _tokenContract.GetUserBalance(TestAccount, symbol);
            afterBalance.ShouldBe(balance + amount);
            
            var afterTokenInfo = _tokenContract.GetTokenInfo(symbol);
            afterTokenInfo.Issued.ShouldBe(tokenInfo.Issued + amount);
            afterTokenInfo.Supply.ShouldBe(tokenInfo.Supply + amount);
        }
        
        [TestMethod]
        public void Issue_Balance2_Test()
        {
            var balance = _tokenContract.GetUserBalance(TestAccount, "ADF");
            var tokenInfo = _tokenContract.GetTokenInfo("ADF");
            var issueInput3 = new IssueInput()
            {
                Amount = 1,
                Symbol = "ADF",
                To = Address.FromBase58(TestAccount),
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput3);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            
            var afterBalance = _tokenContract.GetUserBalance(TestAccount, "ADF");
            afterBalance.ShouldBe(balance + 1);
            
            var afterTokenInfo = _tokenContract.GetTokenInfo("ADF");
            afterTokenInfo.Issued.ShouldBe(tokenInfo.Issued + 1);
            afterTokenInfo.Supply.ShouldBe(tokenInfo.Supply + 1);
        }
        
        [TestMethod]
        public void Issue_Balance3_Test()
        {
            var balance1 = _tokenContract.GetUserBalance(InitAccount,"ADF");  
            var balance2 = _tokenContract.GetUserBalance(TestAccount,"ADF"); 
            var result = _tokenContract.IssueBalance(InitAccount, TestAccount, 2, "ADF");

            var afterBalance1 = _tokenContract.GetUserBalance(InitAccount,"ADF");;
            var afterBalance2 = _tokenContract.GetUserBalance(TestAccount,"ADF");;

            /*balance1.ShouldBe(afterBalance1+2);*/
            balance2.ShouldBe(afterBalance2-2);
        }
        
        [TestMethod]
        [DataRow("egvkcsrabsxrwfjjkvrwyhcagbjkgltxuiodbqfmjybvyehrpczjxdrwdveaigtpy")]
        [DataRow("cgljebtzicgjvwocoygxmplgrnphjjkqltsceevnozqaqkgrwgxynmamufrqllqz")]
        [DataRow("")]
        public void Issue_InvalidMemo_Test(string memo)  //FAILED
        {
            var issueInput1 = new IssueInput()
            {
                //public const int MemoMaxLength = 64;
                Amount = 1,
                Memo = memo,
                Symbol = "ADF",
                To = new Address()
            };
            //var str =CommonHelper.RandomString(65, true); //Generate Random Test Data
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput1);
            //result.Error.ShouldContain("Invalid memo size.");
            result.Error.ShouldContain("Invalid");
        }

        [TestMethod]
        [DataRow("",2,"")]
        [DataRow(null,2,"")]
        [DataRow("ADF",2000,"")]
        [DataRow("ADF",0,"")]
        [DataRow("ASWQ",10,"")]
        public void Issue_InvalidInput_Test(string symbol, int amount, string memo)//FAILED
        {
            var input1 = new IssueInput()
            {
                Symbol = symbol,
                Amount = amount,
                Memo = memo,
                To = new Address(),
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, input1);
            Console.WriteLine(result);
            //result.Error.ShouldContain("Invalid");
            //result.ToString().ShouldContain("Invalid");
        }
        
        [TestMethod]
        [DataRow("ICZ",1000,true,0)]
        [DataRow("ICI",2000,true,9992731)]
        [DataRow("AELF",3000,false,2)]
        [DataRow("T",3000,false,3)]
        public void create_IssueTestData(string symbol, int totalSupply, bool isBurnable, int issueChainId)
        {
            //prepare test data with wrong chainId 2 and 3 for Issue test
            var createInput9 = new CreateInput()
            {
                Symbol = symbol,
                TokenName = "TokenName1",
                TotalSupply = totalSupply,
                Decimals = 10,
                Issuer = Address.FromBase58(InitAccount),
                IsBurnable = isBurnable,
                IssueChainId = issueChainId

            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Create, createInput9);
        }
        [TestMethod]
        public void Issue_FailedWithWrongChainId_Test()
        {
            //For this test case we should create a new token with wrong chainId first.
            //var chainId = NodeManager.GetChainId();//chainId = "AELF" intChainId = 9992731;
            //var intChainId = ChainHelper.ConvertBase58ToChainId(chainId); 
            //if chainId is 0, it will set context.chainId, ie. 9992731
            //the other chainId are all invalid except 0 and 9992731
            var issueInput1 = new IssueInput()
            {
                Amount = 1,
                Memo = "",
                Symbol = "T",
                To = Address.FromBase58(TestAccount),
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Issue, issueInput1);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Failed);
            result.Error.ShouldContain("Unable to issue token with wrong chainId.");
        }
        
        [TestMethod]
        public void Transfer_success_Test()
        {
            var balance1 = _tokenContract.GetUserBalance(InitAccount,"ADF");
            var balance2 = _tokenContract.GetUserBalance(TestAccount,"ADF");
            var transferInput = new TransferInput()
            {
                Amount = 1,
                Memo = "",
                Symbol = "ADF",
                To = Address.FromBase58(TestAccount),
            };
            var result = _tokenContract.ExecuteMethodWithResult(TokenMethod.Transfer, transferInput);
            var afterBalance1 = _tokenContract.GetUserBalance(InitAccount,"ADF");;
            var afterBalance2 = _tokenContract.GetUserBalance(TestAccount,"ADF");;
            balance1.ShouldBe(afterBalance1+transferInput.Amount);
            balance2.ShouldBe(afterBalance2-transferInput.Amount);
        }
        
        /*
         * /Users/duansale/Documents/github/aelf-automation-test/src/AElfChain.Common/Contracts/TokenContract.cs
         * GetUserBalance(string account, string symbol = "")
         * Users/duansale/Documents/github/aelf-automation-test/src/AElfChain.Common/NodeOption.cs
         * public static string GetTokenSymbol(string symbol)
         * {
         * return symbol == "" ? NativeTokenSymbol : symbol;
         * }
         *
         * when u GetUserBalance, and u don't set symbol, it will set the symbol to be ELF,
         * so after balance should count fee.
         * 
         *
         *
         */
        
        
        [TestMethod]
        public void TransferTest()
        
        {
            var balance1 = _tokenContract.GetUserBalance(InitAccount);
            var balance2 = _tokenContract.GetUserBalance(TestAccount);
            var amount = 10000;

            var result = _tokenContract.TransferBalance(InitAccount,TestAccount,amount);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            
            var afterBalance1 = _tokenContract.GetUserBalance(InitAccount);
            var afterBalance2 = _tokenContract.GetUserBalance(TestAccount);
            var fee = result.GetDefaultTransactionFee();
            balance1.ShouldBe(afterBalance1 + amount - fee );
            balance2.ShouldBe(afterBalance2  - amount );
        }

        [TestMethod]
        public async Task NewStubTest_Call()
        {
            var tokenContractAddress =
                ("WnV9Gv3gioSh3Vgaw8SSB96nV8fWUNxuVozCf6Y14e7RXyGaM").ConvertAddress();
            var tester = new ContractTesterFactory(NodeManager);
            var tokenStub = tester.Create<TokenContractContainer.TokenContractStub>(tokenContractAddress, InitAccount);
            var tokenInfo = await tokenStub.GetTokenInfo.CallAsync(new GetTokenInfoInput
            {
                Symbol = NodeOption.NativeTokenSymbol
            });
            tokenInfo.ShouldNotBeNull();
        }
       
        


        [TestMethod]
        public async Task ChangeIssuer()
        {
            await CreateToken(Symbol, long.MaxValue);
            var symbol = Symbol;
            var amount = 1000_00000000;
            var tokenInfo = _tokenContract.GetTokenInfo(symbol);
            var sub = _genesisContract.GetTokenStub(tokenInfo.Issuer.ToBase58());
            var result = await sub.ChangeTokenIssuer.SendAsync(new ChangeTokenIssuerInput
            {
                NewTokenIssuer = TestAccount.ConvertAddress(),
                Symbol = tokenInfo.Symbol
            });
            result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            tokenInfo = _tokenContract.GetTokenInfo(symbol);
            tokenInfo.Issuer.ShouldBe(TestAccount.ConvertAddress());
            var balance = _tokenContract.GetUserBalance(InitAccount, symbol);
            _tokenContract.SetAccount(TestAccount);
            var issue = _tokenContract.IssueBalance(TestAccount, InitAccount, amount, symbol);
            issue.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            var afterBalance = _tokenContract.GetUserBalance(InitAccount, symbol);
            afterBalance.ShouldBe(balance+amount);
        }

        [TestMethod]
        public async Task TokenCreateTest()
        {
            var symbol = Symbol;
            var amount = 5000000000_000000000;
            var issued = 4000000000_00000000;
            var burned = 1_00000000;
            await CreateToken(symbol,amount);
            var tokenInfo = _tokenContract.GetTokenInfo(symbol);
            await IssueToken(InitAccount,symbol, issued);
            await _tokenSub.Burn.SendAsync(new BurnInput{Symbol = symbol, Amount = burned});
            
            var afterTokenInfo = _tokenContract.GetTokenInfo(symbol);
            afterTokenInfo.TotalSupply.ShouldBe(amount);
            afterTokenInfo.Supply.ShouldBe(issued - burned);
            afterTokenInfo.Issued.ShouldBe(issued + tokenInfo.Issued);
            Logger.Info(afterTokenInfo);
        }

        [TestMethod]
        public async Task AddListConnector()
        {
            var list = new List<string>(){Symbol};
            foreach (var symbol in list)
            {
                await AddConnector(symbol);
            }
        }

        public async Task AddConnector(string symbol)
        {
            var amount = 80000000_0000000000;
            await CreateToken(symbol,amount);
            await IssueToken(InitAccount,symbol, amount);
            var tokenInfo = _tokenContract.GetTokenInfo(symbol);
            Logger.Info(tokenInfo);
            var input = new PairConnectorParam
            {
                NativeWeight = "0.05",
                ResourceWeight = "0.01",
                ResourceConnectorSymbol = symbol,
                NativeVirtualBalance = 100000000_00000000
            };
            var organization = _parliamentContract.GetGenesisOwnerAddress();
            var connectorController = await _tokenConverterSub.GetControllerForManageConnector.CallAsync(new Empty());
            connectorController.ContractAddress.ShouldBe(_parliamentContract.Contract);
            connectorController.OwnerAddress.ShouldBe(organization);
            
            var proposal = _parliamentContract.CreateProposal(_tokenConverterContract.ContractAddress,
                nameof(TokenConverterMethod.AddPairConnector), input, organization, InitAccount);
            var miners = AuthorityManager.GetCurrentMiners();
            _parliamentContract.MinersApproveProposal(proposal, miners);
            var result = _parliamentContract.ReleaseProposal(proposal, InitAccount);
            result.Status.ShouldBe(TransactionResultStatus.Mined);
        }

        [TestMethod]
        public void UpdateConnector()
        {
            var input = new Connector
            {
                Symbol = Symbol,
                VirtualBalance = 100000000_00000000,
                Weight = "0.1"
            };

            var organization = _parliamentContract.GetGenesisOwnerAddress();
            var proposal = _parliamentContract.CreateProposal(_tokenConverterContract.ContractAddress,
                nameof(TokenConverterMethod.UpdateConnector), input, organization, InitAccount);
            var miners = AuthorityManager.GetCurrentMiners();
            _parliamentContract.MinersApproveProposal(proposal, miners);
            var result = _parliamentContract.ReleaseProposal(proposal, InitAccount);
            result.Status.ShouldBe(TransactionResultStatus.Mined);
        }

        [TestMethod]
        public async Task EnableConnector()
        {
            var amount = 4500000000_000000000;
            var symbol = "TEST";
            await IssueToken(InitAccount,symbol, amount);
            var ELFamout = await GetNeededDeposit(amount,symbol);
            Logger.Info($"Need ELF : {ELFamout}");
            if (ELFamout > 0)
            {
                (await _tokenSub.Approve.SendAsync(new ApproveInput
                {
                    Spender = _tokenConverterContract.Contract,
                    Symbol = "ELF",
                    Amount = ELFamout
                })).TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            }
            
            (await _tokenSub.Approve.SendAsync(new ApproveInput
            {
                Spender = _tokenConverterContract.Contract,
                Symbol = symbol,
                Amount = amount
            })).TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var buildInput = new ToBeConnectedTokenInfo
            {
                TokenSymbol = symbol,
                AmountToTokenConvert = amount
            };

            var enableConnector = await _tokenConverterSub.EnableConnector.SendAsync(buildInput);
            enableConnector.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            var tokenConverterBalance = await _tokenSub.GetBalance.CallAsync(new GetBalanceInput
            {
                Symbol = symbol,
                Owner = _tokenConverterContract.Contract
            });
            tokenConverterBalance.Balance.ShouldBe(amount);
        }

        [TestMethod]
        public void BuyConnectSymbol()
        {
            var symbol = "TRAFFIC";
            var amount = 10000_00000000;
            var amountToPay = GetPayAmount(symbol, amount);
            var rate = decimal.Parse(_tokenConverterContract.GetFeeRate());
            var fee = Convert.ToInt64(amountToPay * rate);
            var donateFee = fee.Div(2);
            var burnFee = fee.Sub(donateFee); 
            
            _tokenContract.TransferBalance(InitAccount, Account, 1000_00000000, "ELF");
            var balance = _tokenContract.GetUserBalance(Account);
            var resBalance = _tokenContract.GetUserBalance(Account,symbol);

            var tokenConverterBalance = _tokenContract.GetUserBalance(_tokenConverterContract.ContractAddress);
            var treasury = _genesisContract.GetTreasuryContract();
            var dividends = treasury.GetCurrentTreasuryBalance();
            var treasuryDonate = dividends.Value["ELF"];
            var tokenInfo = _tokenContract.GetTokenInfo("ELF");
            
            Logger.Info($"amountToPay={amountToPay}, fee={fee}, donateFee={donateFee}, burnFee={burnFee}");
            Logger.Info($"tokenConverterBalance={tokenConverterBalance},user balance={balance}, treasuryDonate={treasuryDonate}");

            var result = _tokenConverterContract.Buy(Account, symbol, amount);
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var sizeFee = result.GetDefaultTransactionFee();
            var burnAmount = sizeFee.Div(10);
            var transferAmount = sizeFee.Sub(burnAmount);

            NodeManager.WaitOneBlock(result.BlockNumber);
            
            var afterBalance = _tokenContract.GetUserBalance(Account);
            var afterResBalance = _tokenContract.GetUserBalance(Account,symbol);

            var afterTokenConverterBalance = _tokenContract.GetUserBalance(_tokenConverterContract.ContractAddress);
            var afterDividends = treasury.GetCurrentTreasuryBalance();
            var afterTreasuryDonate = afterDividends.Value["ELF"];
            var afterTokenInfo = _tokenContract.GetTokenInfo("ELF");

            afterResBalance.ShouldBe(resBalance + amount);
            afterBalance.ShouldBe(balance - amountToPay - fee - sizeFee);
            afterTokenConverterBalance.ShouldBe(tokenConverterBalance  + amountToPay);
            afterTreasuryDonate.ShouldBe(treasuryDonate + transferAmount + donateFee);
            afterTokenInfo.Supply.ShouldBe(tokenInfo.Supply - burnAmount - burnFee);
            
            Logger.Info($"sizeFee={sizeFee}, burnAmount={burnAmount}, transferAmount={transferAmount}, afterTreasuryDonate={afterTreasuryDonate}");
            Logger.Info($"afterTokenConverterBalance={afterTokenConverterBalance}, balance={afterBalance}");
        }


        [TestMethod]
        public async Task GetCalculateFeeCoefficientOfDeveloper()
        {
            /*
            [pbr::OriginalName("READ")] Read = 0,
            [pbr::OriginalName("STORAGE")] Storage = 1,
            [pbr::OriginalName("WRITE")] Write = 2,
            [pbr::OriginalName("TRAFFIC")] Traffic = 3,
            [pbr::OriginalName("TX")] Tx = 4,
             */
            var result = await _tokenSub.GetCalculateFeeCoefficientsForContract.CallAsync(new Int32Value {Value = 0});
            Logger.Info($"{result}");

            var result1 = await _tokenSub.GetCalculateFeeCoefficientsForContract.CallAsync(new Int32Value {Value = 1});
            Logger.Info($"{result1}");

            var result2 = await _tokenSub.GetCalculateFeeCoefficientsForContract.CallAsync(new Int32Value {Value = 2});
            Logger.Info($"{result2}");

            var result3 = await _tokenSub.GetCalculateFeeCoefficientsForContract.CallAsync(new Int32Value {Value = 3});
            Logger.Info($"{result3}");
        }

        [TestMethod]
        public async Task GetCalculateFeeCoefficientOfUser()
        {
            var result = await _tokenSub.GetCalculateFeeCoefficientsForSender.CallAsync(new Empty());
            Logger.Info($"{result}");
        }

        [TestMethod]
        public async Task GetBasicToken()
        {
            var result = await _tokenConverterSub.GetBaseTokenSymbol.CallAsync(new Empty());
            Logger.Info($"{result.Symbol}");
        }

        [TestMethod]
        public async Task ChangeManagerAddress()
        {
            var manager = await _tokenConverterSub.GetControllerForManageConnector.CallAsync(new Empty());
            var proposer = AuthorityManager.GetCurrentMiners().First();
            Logger.Info($"manager is {manager.OwnerAddress}");
            var association = _genesisContract.GetAssociationAuthContract();
            var newController = AuthorityManager.CreateAssociationOrganization();
            var input = new AuthorityInfo
            {
                OwnerAddress = newController,
                ContractAddress = association.Contract
            };

            var change = AuthorityManager.ExecuteTransactionWithAuthority(_tokenConverterContract.ContractAddress,
                nameof(TokenConverterMethod.ChangeConnectorController), input, proposer,
                manager.OwnerAddress);
            change.Status.ShouldBe(TransactionResultStatus.Mined);
            var newManager = await _tokenConverterSub.GetControllerForManageConnector.CallAsync(new Empty());
            newManager.ContractAddress.ShouldBe(association.Contract);
            newManager.OwnerAddress.ShouldBe(newController);
        }

        [TestMethod]
        public async Task GetManagerAddress()
        {
            var manager = await _tokenConverterSub.GetControllerForManageConnector.CallAsync(new Empty());
            Logger.Info($"manager is {manager.OwnerAddress}");
            var organization = _parliamentContract.GetGenesisOwnerAddress();
            Logger.Info($"organization is {organization}");
        }

        [TestMethod]
        public async Task GetConnector()
        {
            var result = await _tokenConverterSub.GetPairConnector.CallAsync(new TokenSymbol {Symbol = "STA"});
            Logger.Info($"{result}");
        }

        [TestMethod]
        public async Task GetNeededDepositTest()
        {
            var amount = 4500000000_000000000;
            var symbol = Symbol;
            var deposit = await GetNeededDeposit(amount,symbol);
            Logger.Info($"{deposit}");
        }

        [TestMethod]
        public async Task CheckPrice()
        {
            var symbol = "CPU";
            var amount = 1_00000000;

            var result = await _tokenConverterSub.GetPairConnector.CallAsync(new TokenSymbol {Symbol = symbol});
            var fromConnectorWeight = decimal.Parse(result.DepositConnector.Weight);
            var toConnectorWeight = decimal.Parse(result.ResourceConnector.Weight);
            
            var amountToPay = BancorHelper.GetAmountToPayFromReturn(
                GetSelfBalance(result.DepositConnector,result.DepositConnector.RelatedSymbol), fromConnectorWeight,
                GetSelfBalance(result.ResourceConnector,symbol), toConnectorWeight,
                amount);
            var rate = decimal.Parse(_tokenConverterContract.GetFeeRate());
            var fee = Convert.ToInt64(amountToPay * rate);
            var amountToPayPlusFee = amountToPay.Add(fee);
            
            Logger.Info($"amountToPay: {amountToPay} fee: {fee}, amountToPayPlusFee {amountToPayPlusFee}");
        }

        private long GetPayAmount(string symbol, long amount)
        {
            var result = _tokenConverterContract.GetPairConnector(symbol);
            var fromConnectorWeight = decimal.Parse(result.DepositConnector.Weight);
            var toConnectorWeight = decimal.Parse(result.ResourceConnector.Weight);
            
            var amountToPay = BancorHelper.GetAmountToPayFromReturn(
                GetSelfBalance(result.DepositConnector,symbol), fromConnectorWeight,
                GetSelfBalance(result.ResourceConnector,symbol), toConnectorWeight,
                amount);
            
            return amountToPay;
        }

        [TestMethod]
        public async Task Check()
        {
            var amount = 70000000000_00000000L;
            var symbol = "RES";
            var tokenInfo = _tokenContract.GetTokenInfo(symbol);
            var balance = _tokenContract.GetUserBalance(_tokenConverterContract.ContractAddress, symbol);
            var amountOutOfTokenConvert = tokenInfo.TotalSupply - balance - amount;

            var result = await _tokenConverterSub.GetPairConnector.CallAsync(new TokenSymbol {Symbol = symbol});
            var fb = result.DepositConnector.VirtualBalance;
            var tb = result.ResourceConnector.IsVirtualBalanceEnabled ? result.ResourceConnector.VirtualBalance.Add(tokenInfo.TotalSupply)
                : tokenInfo.TotalSupply;
            var fromConnectorWeight = decimal.Parse(result.DepositConnector.Weight);
            var toConnectorWeight = decimal.Parse(result.ResourceConnector.Weight);
            decimal bt = tb;
            decimal a = amountOutOfTokenConvert;
            decimal wf = fromConnectorWeight;
            decimal wt = toConnectorWeight;
            decimal x = bt / (bt - a);
            decimal y = wt / wf;
            
            var needDeposit =
                BancorHelper.GetAmountToPayFromReturn(fb, fromConnectorWeight,
                    tb, toConnectorWeight, amountOutOfTokenConvert);
            Logger.Info(needDeposit);
        }
        
        public long GetSelfBalance(Connector connector,string symbol)
        {
            long realBalance;
            if (connector.IsDepositAccount)
            {
                var deposit = _tokenConverterContract.GetDepositConnectorBalance(symbol);
                var virtualBalance = connector.VirtualBalance;
                realBalance = deposit - virtualBalance;
            }
            else
            {
                realBalance = _tokenContract.GetUserBalance(_tokenConverterContract.ContractAddress, connector.Symbol);
            }

            if (connector.IsVirtualBalanceEnabled)
            {
                return connector.VirtualBalance.Add(realBalance);
            }

            return realBalance;
        }

        [TestMethod]
        [DataRow("2WHXRoLRjbUTDQsuqR5CntygVfnDb125qdJkudev4kVNbLhTdG")]
        public async Task Acs8ContractTest(string acs8Contract)
        {
            foreach (var s in ResourceSymbol)
            {
                var balance = await _tokenSub.GetBalance.CallAsync(new GetBalanceInput
                    {Owner = acs8Contract.ConvertAddress(), Symbol = s});
                Logger.Info($"{s} balance is {balance.Balance}");
            }
        }

        [TestMethod]
        public  async Task BurnToken()
        {
            var amount = 10_0000000;
            foreach (var s in ResourceSymbol)
            {
                var balance = await _tokenSub.GetBalance.CallAsync(new GetBalanceInput
                    {Owner = InitAccount.ConvertAddress(), Symbol = s});
                var result = await _tokenSub.Burn.SendAsync(new BurnInput
                {
                    Symbol = s,
                    Amount = amount
                });
                result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
                Logger.Info($"{s}: {balance}");
                var tokenInfo = await _tokenSub.GetTokenInfo.CallAsync(new GetTokenInfoInput{Symbol = s});
                Logger.Info($"{s}: {tokenInfo}");
            }
            
            foreach (var s in ResourceSymbol)
            {
                var balance = await _tokenSub.GetBalance.CallAsync(new GetBalanceInput
                    {Owner = InitAccount.ConvertAddress(), Symbol = s});
                Logger.Info($"{s}: {balance}");
            }
        }
        
        [TestMethod]
        public  async Task BurnToken_OneToken()
        {
            var s = "ELF";
            var amount = 39824688;

            var balance = await _tokenSub.GetBalance.CallAsync(new GetBalanceInput
                {Owner = InitAccount.ConvertAddress(), Symbol = s});
            var result = await _tokenSub.Burn.SendAsync(new BurnInput
            {
                Symbol = s,
                Amount = amount
            });
            result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            Logger.Info($"{s}: {balance}");
            var tokenInfo = await _tokenSub.GetTokenInfo.CallAsync(new GetTokenInfoInput{Symbol = s});
            Logger.Info($"{s}: {tokenInfo}");
            var afterBalance = await _tokenSub.GetBalance.CallAsync(new GetBalanceInput
                {Owner = InitAccount.ConvertAddress(), Symbol = s});
            Logger.Info($"{s}: {afterBalance}");
        }

        [TestMethod]
        [DataRow("AEUSD",long.MaxValue)]
        public async Task CreateToken(string symbol,long amount)
        {
            var voteContract = _genesisContract.GetVoteContract(InitAccount);
            if (!_tokenContract.GetTokenInfo(symbol).Equals(new TokenInfo())) return;
            var result = await _tokenSub.Create.SendAsync(new CreateInput
            {
                Issuer = InitAccount.ConvertAddress(),
                Symbol = symbol,
                Decimals = 3,
                IsBurnable = true,
                TokenName = $"{symbol} symbol",
                TotalSupply = amount,
                LockWhiteList =
                {
                    voteContract.Contract
                }
            });

            result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        }

        [TestMethod]
        [DataRow("28Y8JA1i2cN6oHvdv7EraXJr9a1gY6D1PpJXw9QtRMRwKcBQMK","AEUSD",1000000000_000)]
        public async Task IssueToken(string account,string symbol, long amount)
        {
            var balance = _tokenContract.GetUserBalance(account, symbol);
            var issueResult = await _tokenSub.Issue.SendAsync(new IssueInput
            {
                Amount = amount,
                Symbol = symbol,
                To =  account.ConvertAddress()
            });
            issueResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            var afterBalance = _tokenContract.GetUserBalance(account, symbol);
            afterBalance.ShouldBe(amount + balance);
        }

        private async Task<long> GetNeededDeposit(long amount,string symbol)
        {
            var result = await _tokenConverterSub.GetNeededDeposit.CallAsync(new ToBeConnectedTokenInfo
            {
                TokenSymbol = symbol,
                AmountToTokenConvert = amount
            });
            return result.NeedAmount;
        }
    }
}