// ════════════════════════════════════════════════════════════════════════════
// wwwroot/js/site.js — small scripts for the public site, loaded on every page
// (see the <script> tag at the bottom of _Layout.cshtml).
//
// Currently:
//   • copy buttons, such as "Copy" next to the Discord name in the footer.
//     Any button written like this gets the behavior:
//       <button data-copy="text to copy" data-copied-text="Copied!">Copy</button>
//   • the cookie notice (Views/Shared/_CookieNotice.cshtml)
// ════════════════════════════════════════════════════════════════════════════

// ── Cookie notice ──
// Show it unless the visitor clicked OK before. The choice is kept in
// localStorage (stays in this browser, never sent to the server). try/catch:
// some browsers block localStorage (e.g. private mode); then the notice just
// shows on each visit and OK hides it for this page.
const cookieNotice = document.getElementById('cookie-notice');
const cookieKey = 'cookie-notice-ok';
if (cookieNotice) {
    let seen = false;
    try { seen = localStorage.getItem(cookieKey) === '1'; } catch { }
    cookieNotice.hidden = seen;

    cookieNotice.querySelector('[data-cookie-ok]')?.addEventListener('click', () => {
        try { localStorage.setItem(cookieKey, '1'); } catch { }
        cookieNotice.hidden = true;
    });
}

// One click listener on the whole document ("event delegation"): it catches
// clicks anywhere, then checks whether a copy button was clicked. This also
// works for buttons added to the page later.
document.addEventListener('click', async e => {
    // closest() finds the button even if you clicked something inside it.
    const button = e.target.closest('button[data-copy]');
    if (!button) return;

    if (!(await copyText(button.dataset.copy))) return;

    // Show "Copied!" for 2 seconds (2000 milliseconds), then the original text.
    const original = button.textContent;
    button.textContent = button.dataset.copiedText || 'Copied!';
    setTimeout(() => { button.textContent = original; }, 2000);
});

// Copies text to the clipboard. Returns true if it worked.
async function copyText(text) {
    try {
        // The modern way. The browser may refuse it (e.g. on plain http on
        // another host, or when the window doesn't have focus).
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        // Fallback where the Clipboard API is unavailable or not allowed:
        // put the text in an invisible text box, select it and use the
        // older "copy" command, then remove the box again.
        const area = document.createElement('textarea');
        area.value = text;
        area.setAttribute('readonly', '');
        area.style.position = 'fixed';
        area.style.opacity = '0';
        document.body.appendChild(area);
        area.select();
        const ok = document.execCommand('copy');
        area.remove();
        return ok;
    }
}
