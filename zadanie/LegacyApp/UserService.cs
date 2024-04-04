using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository = new();
        private readonly UserCreditService _userCreditService = new();

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidUser(firstName, lastName, email, dateOfBirth))
                return false;

            var client = _clientRepository.GetById(clientId);
            if (client == null)
                return false;

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            if (!IsCreditLimitSufficient(user))
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidUser(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return false;
            
            int age = CalculateAge(dateOfBirth);
            if (age < 21)
                return false;
            
            if (!email.Contains("@") || !email.Contains("."))
                return false;

            

            return true;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var realtime = DateTime.Now;
            int age = realtime.Year - dateOfBirth.Year;
            if (realtime.Month < dateOfBirth.Month || (realtime.Month == dateOfBirth.Month && realtime.Day < dateOfBirth.Day))
                age--;
            return age;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                user.HasCreditLimit = true;
                int creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                if (client.Type == "ImportantClient")
                    creditLimit *= 2;
                user.CreditLimit = creditLimit;
            }

            return user;
        }

        private bool IsCreditLimitSufficient(User user)
        {
            if (user.HasCreditLimit && user.CreditLimit < 500)
                return false;
            return true;
        }

        
    }
}
