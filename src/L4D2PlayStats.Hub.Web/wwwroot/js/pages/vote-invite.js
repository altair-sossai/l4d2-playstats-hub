var voteInviteConfig = JSON.parse(document.getElementById("vote-invite-config").textContent);

(function () {
    var key = 'vote-invite-hidden-until';
    var invite = document.getElementById('vote-invite');
    var hiddenUntil = 0;

    try {
        hiddenUntil = parseInt(localStorage.getItem(key) || '0', 10);
    } catch (e) {
    }

    if (Date.now() >= hiddenUntil)
        invite.classList.remove('hidden');

    invite.querySelector('.vote-invite-close').addEventListener('click', function () {
        invite.classList.add('hidden');

        try {
            localStorage.setItem(key, String(Date.now() + voteInviteConfig.values.hiddenMilliseconds));
        } catch (e) {
        }
    });
})();
