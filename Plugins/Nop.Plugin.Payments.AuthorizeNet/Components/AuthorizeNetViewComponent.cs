﻿using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Payments.AuthorizeNet.Controllers;
using Nop.Plugin.Payments.AuthorizeNet.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.AuthorizeNet.Components
{

    [ViewComponent(Name = "AuthorizeNet")]
    public class AuthorizeNetViewComponent : NopViewComponent
    {
 
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year
                });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                var text = i < 10 ? "0" + i : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = text
                });
            }

            //set postback values (we cannot access "Form" with "GET" requests)
            if (Request.Method == WebRequestMethods.Http.Get)
                return View("~/Plugins/Payments.AuthorizeNet/Views/PaymentInfo.cshtml", model);

            var form = Request.Form;
            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];
            var selectedMonth = model.ExpireMonths.FirstOrDefault(x =>
                x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));

            if (selectedMonth != null)
                selectedMonth.Selected = true;

            var selectedYear = model.ExpireYears.FirstOrDefault(x =>
                x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));

            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("~/Plugins/Payments.AuthorizeNet/Views/PaymentInfo.cshtml", model);
        }
    }
}
