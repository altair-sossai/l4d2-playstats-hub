var layout1Config = JSON.parse(document.getElementById("layout-1-config").textContent);


function imgOnError(img) {
    img.onerror = null;
    img.src = layout1Config.values.emptyAvatarUrl;
}

function copyTextToClipboard(text, sender) {
    var $this = $(sender);

    navigator.clipboard.writeText(text)
        .then(function(){
            $this.popover("show");

            setTimeout(function () {
                $this.popover("hide");
            }, 1500);
        })
        .catch(function () {
            alert(layout1Config.values.copyFailed);
        });
}
