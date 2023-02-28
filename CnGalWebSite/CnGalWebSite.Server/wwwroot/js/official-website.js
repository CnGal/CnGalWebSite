
var dotNetHelper;

async function onWebsiteTemplateInit(helper)
{
    dotNetHelper = helper;
    console.log(`init`);
    var msg = await dotNetHelper.invokeMethodAsync('CheckBooking');
    console.log(msg);
    msg = await dotNetHelper.invokeMethodAsync('GetEntryModel');
    console.log(msg);
}
