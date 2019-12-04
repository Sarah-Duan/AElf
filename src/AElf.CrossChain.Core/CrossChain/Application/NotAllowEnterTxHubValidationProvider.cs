using System.Threading.Tasks;
using AElf.Contracts.CrossChain;
using AElf.Kernel.SmartContract.Application;
using AElf.Kernel.SmartContractExecution.Application;
using AElf.Types;

namespace AElf.CrossChain
{
    public class NotAllowEnterTxHubValidationProvider : ITransactionValidationProvider
    {
        public bool ValidateWhileSyncing => false;

        private readonly ISmartContractAddressService _smartContractAddressService;

        public NotAllowEnterTxHubValidationProvider(ISmartContractAddressService smartContractAddressService)
        {
            _smartContractAddressService = smartContractAddressService;
        }

        public Task<bool> ValidateTransactionAsync(Transaction transaction)
        {
            var crossChainContractAddress =
                _smartContractAddressService.GetAddressByContractName(CrossChainSmartContractAddressNameProvider.Name);

            return Task.FromResult(transaction.To != crossChainContractAddress ||
                                   transaction.MethodName !=
                                   nameof(CrossChainContractContainer.CrossChainContractStub.RecordCrossChainData));
        }
    }
}