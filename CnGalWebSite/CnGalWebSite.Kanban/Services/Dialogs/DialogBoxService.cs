using Live2DTest.DataRepositories;
using CnGalWebSite.Kanban.Components.Dialogs;
using CnGalWebSite.Kanban.Extensions;
using CnGalWebSite.Kanban.Models;
using CnGalWebSite.Kanban.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Dialogs
{
    public class DialogBoxService:IDialogBoxService
    {
        private readonly ILive2DService _live2DService;
        private readonly IRepository<ClothesModel> _clothesRepository;
        private readonly IRepository<ExpressionModel> _expressionRepository;
        private readonly IRepository<MotionGroupModel> _motionGroupRepository;

        private DialogBoxCard _dialogBoxCard;

        public DialogBoxService(ILive2DService live2DService, IRepository<ClothesModel> clothesRepository, IRepository<ExpressionModel> expressionRepository, IRepository<MotionGroupModel> motionGroupRepository)
        {
            _live2DService = live2DService;
            _clothesRepository = clothesRepository;
            _expressionRepository = expressionRepository;
            _motionGroupRepository = motionGroupRepository;
        }

        public void  Init(DialogBoxCard dialogBoxCard)
        {
            _dialogBoxCard = dialogBoxCard;
        }

        public async Task ShowDialogBox(DialogBoxModel model)
        {
            await _dialogBoxCard?.ShowDialogBox(model);
        }

        public async Task ShowDialogBox(string content)
        {
           await ShowDialogBox(StringToDialogModel(content));
        }

        public DialogBoxModel StringToDialogModel(string content)
        {
            var model=new DialogBoxModel();

            while(content.StartsWith('['))
            {
                var str = content.MidStrEx("[", "]");
                content = content.Replace("[" + str + "]", "");
                var index = str[0];
                str = str.Remove(0, 1).Trim();
                switch (index)
                {
                    case '0':
                        var exp= _expressionRepository.GetAll().FirstOrDefault(s => s.DisplayName == str);
                        model.Expression = _expressionRepository.GetAll().IndexOf(exp);
                        break;
                    case '1':
                        var motionGroup = "";
                        var motionId =0;
                        foreach (var item in _motionGroupRepository.GetAll())
                        {
                            var motion= item.Motions.FirstOrDefault(s => s.Name == str);
                            if(motion!=null)
                            {
                                motionGroup = item.Name;
                                motionId = motion.Id;
                                break;
                            }
                        }
                        model.Motion = motionId;
                        model.MotionGroup = motionGroup;
                        break;
                    case '2':
                        str = str.ToLower();
                        if (str=="success")
                        {
                            model.Type = DialogBoxType.Success;
                        }
                        else if (str == "info")
                        {
                            model.Type = DialogBoxType.Info;
                        }
                        else if (str == "warning")
                        {
                            model.Type = DialogBoxType.Warning;
                        }
                        else if (str == "error")
                        {
                            model.Type = DialogBoxType.Error;
                        }
                        break;
                }
            }

            model.Content = content;
            return model;
        }
    }
}
