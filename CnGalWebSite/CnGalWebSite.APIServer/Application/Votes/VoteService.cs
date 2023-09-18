
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;


namespace CnGalWebSite.APIServer.Application.Votes
{
    public class VoteService : IVoteService
    {
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IAppHelper _appHelper;

        public VoteService(IAppHelper appHelper, IRepository<Vote, long> voteRepository)
        {
            _voteRepository = voteRepository;
            _appHelper = appHelper;
        }

    }
}
