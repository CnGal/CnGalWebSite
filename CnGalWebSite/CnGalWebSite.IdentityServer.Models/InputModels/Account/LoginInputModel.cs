// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.InputModels.Account
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        public string Username { get; set; }
        [Required(ErrorMessage = "请输入密码")]
        public string Password { get; set; }
        public bool RememberLogin { get; set; } = true;
        public string ReturnUrl { get; set; }

        public HumanMachineVerificationResult VerifyResult { get; set; } = new HumanMachineVerificationResult();

    }
}
