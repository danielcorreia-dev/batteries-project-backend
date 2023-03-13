using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Results
{
    public class SigninResponseModel
    {
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public Guid RefreshToken { get; set; }

        public SigninResponseModel(User user, string accessToken, Guid refreshToken)
        {
            Name = user.Name;
            AccessToken = accessToken;
            RefreshToken = refreshToken;

        }

    }
}
