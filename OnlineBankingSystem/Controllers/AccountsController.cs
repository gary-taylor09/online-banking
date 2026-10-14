using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBakingSystem.Database;
using OnlineBankingSystem.Database;
using OnlineBankingSystem.RequestsResponses;
using System.Security.Claims;

namespace OnlineBakingSystem.Controllers
{
    public class AccountsController : Controller
    {
        UserStore<IdentityUser> _userStore;
        RoleStore<IdentityRole> _roleStore;
        BankingDbContext _bankingDb;

        public AccountsController(UserStore<IdentityUser> userStore, RoleStore<IdentityRole> roleStore, BankingDbContext bankingDb)
        {
            _userStore = userStore;
            _roleStore = roleStore;
            _bankingDb = bankingDb;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountsForUser(string userId)
        {
            var user = await _userStore.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != authenticatedUserId  && (!User.IsInRole("Admin") || !User.IsInRole("Clerk")))
            {
                return Forbid("You are not authorized to view this user's accounts");
            }

            var accounts = _bankingDb.Accounts.Where(a => a.UserId == userId && !a.Deleted).ToList();
            return Ok(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        {
            var user = await _userStore.FindByIdAsync(request.RequestUserId);
            if (user == null)
            {
                return NotFound("Employee user not found");
            }

            if (request.RequestUserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && (!User.IsInRole("Admin") || !User.IsInRole("Clerk")))
            {
                return Forbid("You are not authorized to create an account for this user");
            }

            var account = await _bankingDb.Accounts.AddAsync(new Account
            {
                UserId = request.AccountHolderUserId,
                AccountNumber = GenerateAccountNumber(),
                Balance = request.InitialDeposit,
                AccountType = request.AccountType
            });

            await _bankingDb.SaveChangesAsync();

            return Ok(new CreateAccountResponse
            {
                Success = true,
                Message = "Account created successfully",
                AccountId = account.Entity.Id,
                AccountNumber = account.Entity.AccountNumber
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
        {
            try
            {
                // Check authorization - only Admin and Clerk roles
                if (!User.IsInRole("Admin") && !User.IsInRole("Clerk"))
                {
                    return Forbid("You are not authorized to delete accounts. Only Admin and Clerk roles can perform this action.");
                }

                var account = await _bankingDb.Accounts.FindAsync(request.AccountId);
                if (account == null)
                {
                    return NotFound("Account not found");
                }

                account.Deleted = true;
                await _bankingDb.SaveChangesAsync();

                return Ok(new DeleteAccountResponse
                {
                    Success = true,
                    Message = "Account deleted successfully",
                    AccountId = request.AccountId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    Success = false,
                    Message = $"An error occurred while deleting the account: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionRequest request)
        {
            try
            {
                // Check authorization - Admin, Clerk, and AccountManager roles
                if (!User.IsInRole("Admin") && !User.IsInRole("Clerk") && !User.IsInRole("AccountManager"))
                {
                    return Forbid("You are not authorized to create transactions. Only Admin, Clerk, and AccountManager roles can perform this action.");
                }

                var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var account = await _bankingDb.Accounts.FindAsync(request.AccountId);
                if (account == null || account.Deleted)
                {
                    return NotFound("Account not found");
                }

                // If not Admin or Clerk, restrict to their own account
                if (!User.IsInRole("Admin") && !User.IsInRole("Clerk"))
                {
                    if (account.UserId != authenticatedUserId)
                    {
                        return Forbid("You are not authorized to create transactions for this account");
                    }
                }

                // Get the transaction type
                var transactionType = await _bankingDb.TransactionTypes
                    .FirstOrDefaultAsync<TransactionType>(t => t.Name == request.TransactionType);
                if (transactionType == null)
                {
                    return BadRequest($"Transaction type '{request.TransactionType}' not found");
                }

                // Handle transfer transactions
                Account transferToAccount = null;
                if (request.TransactionType == "Transfer" && request.TransferToAccountId.HasValue)
                {
                    transferToAccount = await _bankingDb.Accounts.FindAsync(request.TransferToAccountId.Value);
                    if (transferToAccount == null || transferToAccount.Deleted)
                    {
                        return BadRequest("Transfer destination account not found");
                    }
                }

                // Validate sufficient balance for withdrawal/transfer
                if ((request.TransactionType == "Withdrawal" || request.TransactionType == "Transfer") &&
                    account.Balance < request.Amount)
                {
                    return BadRequest("Insufficient funds for this transaction");
                }

                // Create the transaction
                var transaction = new Transaction
                {
                    AccountId = request.AccountId,
                    Amount = request.Amount,
                    Description = request.Description,
                    Type = transactionType,
                    TransactionTypeID = transactionType.ID,
                    Timestamp = DateTimeOffset.UtcNow
                };

                await _bankingDb.Transactions.AddAsync(transaction);

                // Update account balance
                if (request.TransactionType == "Withdrawal" || request.TransactionType == "Transfer")
                {
                    account.Balance -= request.Amount;
                }
                else if (request.TransactionType == "Deposit")
                {
                    account.Balance += request.Amount;
                }

                // Handle transfer to another account
                if (transferToAccount != null)
                {
                    transferToAccount.Balance += request.Amount;
                }

                await _bankingDb.SaveChangesAsync();

                return Ok(new CreateTransactionResponse
                {
                    Success = true,
                    Message = "Transaction created successfully",
                    TransactionId = transaction.ID,
                    AccountId = request.AccountId,
                    Amount = request.Amount,
                    Timestamp = transaction.Timestamp.DateTime,
                    NewAccountBalance = account.Balance
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    Success = false,
                    Message = $"An error occurred while creating the transaction: {ex.Message}"
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Reconciliation(ReconciliationRequest request)
        {
            try
            {
                // Check authorization - only Admin and Clerk roles
                if (!User.IsInRole("Admin") && !User.IsInRole("Clerk"))
                {
                    return Forbid("You are not authorized to perform reconciliation. Only Admin and Clerk roles can perform this action.");
                }

                var transaction = await _bankingDb.Transactions.FindAsync(request.TransactionId);
                if (transaction == null || transaction.Deleted)
                {
                    return NotFound("Transaction not found");
                }

                var account = await _bankingDb.Accounts.FindAsync(transaction.AccountId);
                if (account == null || account.Deleted)
                {
                    return NotFound("Associated account not found");
                }

                decimal oldAmount = transaction.Amount;
                decimal difference = request.NewAmount - oldAmount;

                // Update transaction amount
                transaction.Amount = request.NewAmount;

                // Adjust account balance based on the difference
                account.Balance += difference;

                await _bankingDb.SaveChangesAsync();

                return Ok(new ReconciliationResponse
                {
                    Success = true,
                    Message = "Transaction reconciliation completed successfully",
                    TransactionId = request.TransactionId,
                    OldAmount = oldAmount,
                    NewAmount = request.NewAmount,
                    AccountBalanceAdjustment = difference
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse
                {
                    Success = false,
                    Message = $"An error occurred while performing reconciliation: {ex.Message}"
                });
            }
        }

        private string GenerateAccountNumber()
        {
            var latestAccountNumber = _bankingDb.Accounts.Where(a => !a.Deleted).OrderByDescending(a => a.Id).FirstOrDefault()?.AccountNumber;
            if (string.IsNullOrEmpty(latestAccountNumber))
            {
                return "ACC000001";
            }

            var lastNumber = int.Parse(latestAccountNumber.Substring(3));
            return $"ACC{lastNumber + 1:000000}";
        }
    }
}
