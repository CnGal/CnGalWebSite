
function slotMachineTrigger(luckyNum) {
    let slot1 = document.querySelector("div.slot1 > ul");
    let slot2 = document.querySelector("div.slot2 > ul");
    let slot3 = document.querySelector("div.slot3 > ul");
    let slot4 = document.querySelector("div.slot4 > ul");
    if (slot1.querySelectorAll('li').length > 1) {
        let lis1 = Array.prototype.slice.call(slot1.querySelectorAll('li')).slice(0, 30);
        let lis2 = Array.prototype.slice.call(slot2.querySelectorAll('li')).slice(0, 30);
        let lis3 = Array.prototype.slice.call(slot3.querySelectorAll('li')).slice(0, 30);
        let lis4 = Array.prototype.slice.call(slot4.querySelectorAll('li')).slice(0, 30);
        for (let li of lis1) {
            slot1.removeChild(li);
        }
        for (let li of lis2) {
            slot2.removeChild(li);
        }
        for (let li of lis3) {
            slot3.removeChild(li);
        }
        for (let li of lis4) {
            slot4.removeChild(li);
        }
        slot1.classList.remove("slotAnimation");
        slot2.classList.remove("slotAnimation");
        slot3.classList.remove("slotAnimation");
        slot4.classList.remove("slotAnimation");
        slot1.style.transform = "translateY(0px)";
        slot2.style.transform = "translateY(0px)";
        slot3.style.transform = "translateY(0px)";
        slot4.style.transform = "translateY(0px)";
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
        let li1 = document.createElement('li');
        let li2 = document.createElement('li');
        let li3 = document.createElement('li');
        let li4 = document.createElement('li');
        li1.textContent = n1;
        li2.textContent = n2;
        li3.textContent = n3;
        li4.textContent = n4;
        slot1.appendChild(li1);
        slot2.appendChild(li2);
        slot3.appendChild(li3);
        slot4.appendChild(li4);
    }
    luckyNum = luckyNum.toString();
    while (luckyNum.length < 4) {
        luckyNum = '0' + luckyNum;
    }
    let li5 = document.createElement('li');
    let li6 = document.createElement('li');
    let li7 = document.createElement('li');
    let li8 = document.createElement('li');
    li5.textContent = luckyNum[0];
    li6.textContent = luckyNum[1];
    li7.textContent = luckyNum[2];
    li8.textContent = luckyNum[3];
    slot1.appendChild(li5);
    slot2.appendChild(li6);
    slot3.appendChild(li7);
    slot4.appendChild(li8);
    slot1.classList.add("slotAnimation");
    slot2.classList.add("slotAnimation");
    slot3.classList.add("slotAnimation");
    slot4.classList.add("slotAnimation");
    let fin = function () {
        slot1.style.transform = "translateY(-4890px)";
        slot2.style.transform = "translateY(-4890px)";
        slot3.style.transform = "translateY(-4890px)";
        slot4.style.transform = "translateY(-4890px)";
    };
    setTimeout(fin, 1000);
};

/*获取UA*/
function getUserAgent() {
    return navigator.userAgent;
}
