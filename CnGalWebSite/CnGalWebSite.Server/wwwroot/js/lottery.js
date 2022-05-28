
function slotMachineTrigger(luckyNum) {
    let slot1 = $("div.slot1 > ul");
    let slot2 = $("div.slot2 > ul");
    let slot3 = $("div.slot3 > ul");
    let slot4 = $("div.slot4 > ul");
    if ($(slot1).find('li').length > 1) {
        $(slot1).find('li:lt(30)').remove();
        $(slot2).find('li:lt(30)').remove();
        $(slot3).find('li:lt(30)').remove();
        $(slot4).find('li:lt(30)').remove();
        $(slot1).removeClass("slotAnimation");
        $(slot2).removeClass("slotAnimation");
        $(slot3).removeClass("slotAnimation");
        $(slot4).removeClass("slotAnimation");
        $(slot1).css("transform", "translateY(0px)");
        $(slot2).css("transform", "translateY(0px)");
        $(slot3).css("transform", "translateY(0px)");
        $(slot4).css("transform", "translateY(0px)");
        void document.getElementById('slot1').offsetHeight;
        void document.getElementById('slot2').offsetHeight;
        void document.getElementById('slot3').offsetHeight;
        void document.getElementById('slot4').offsetHeight;
    }
    for (let i = 0; i < 29; i++) {
        let n1 = Math.floor(Math.random() * 10);
        let n2 = Math.floor(Math.random() * 10);
        let n3 = Math.floor(Math.random() * 10);
        let n4 = Math.floor(Math.random() * 10);
        $(slot1).append('<li>' + n1 + '</li>');
        $(slot2).append('<li>' + n2 + '</li>');
        $(slot3).append('<li>' + n3 + '</li>');
        $(slot4).append('<li>' + n4 + '</li>');
    }
    luckyNum = luckyNum.toString();
    while (luckyNum.length < 4) {
        luckyNum = '0' + luckyNum;
    }
    $(slot1).append('<li>' + luckyNum[0] + '</li>');
    $(slot2).append('<li>' + luckyNum[1] + '</li>');
    $(slot3).append('<li>' + luckyNum[2] + '</li>');
    $(slot4).append('<li>' + luckyNum[3] + '</li>');
    $(slot1).addClass("slotAnimation");
    $(slot2).addClass("slotAnimation");
    $(slot3).addClass("slotAnimation");
    $(slot4).addClass("slotAnimation");
    let fin = function () {
        $(slot1).css("transform", "translateY(-4890px)");
        $(slot2).css("transform", "translateY(-4890px)");
        $(slot3).css("transform", "translateY(-4890px)");
        $(slot4).css("transform", "translateY(-4890px)");
    };
    setTimeout(fin, 1000);
};
