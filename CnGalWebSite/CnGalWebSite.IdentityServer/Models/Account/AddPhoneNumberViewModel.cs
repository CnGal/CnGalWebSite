﻿using CnGalWebSite.IdentityServer.Models.Geetest;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class AddPhoneNumberViewModel:AddPhoneNumberInputModel
    {
        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();
    }
}
