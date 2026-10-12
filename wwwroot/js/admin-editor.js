// ════════════════════════════════════════════════════════════════════════════
// wwwroot/js/admin-editor.js — makes the admin editors interactive:
//   • the Markdown toolbar (bold, italic, lists...) and keyboard shortcuts
//   • the Write/Preview tabs
//   • the cover image picker (post editor only)
//   • the image library: upload, drag-and-drop, insert, delete
//
// Used by the post editor and the About editor; it works on any
// <form data-editor>. It runs in the browser, not on the server.
//
// Images are resized and re-encoded in the browser before upload; this keeps
// them small (the database has a 1 MB per-statement limit) and strips
// metadata such as GPS position.
//
// JAVASCRIPT BASICS USED BELOW
//   const x = ...         a variable that won't be reassigned (let = can change)
//   (a, b) => a + b       an "arrow function": a short way to write a function
//   document.querySelector('.css-selector')   find the first matching element
//   element.addEventListener('click', fn)     run fn when the element is clicked
//   element.dataset.foo   reads the HTML attribute data-foo="..."
//   async / await         wait for slow things (network, image decoding)
//                         without freezing the page
//   fetch(url, {...})     send a request to the server from JavaScript
// ════════════════════════════════════════════════════════════════════════════

// The whole file is wrapped in a function that runs immediately:
// (function () { ... })();  This keeps its variables private, so they can't
// clash with other scripts on the page.
(function () {
    // Strict mode: turns some silent JavaScript mistakes into real errors.
    'use strict';

    // Only run on pages that have an editor form.
    const form = document.querySelector('form[data-editor]');
    if (!form) return;

    // The antiforgery token from the form's hidden field. We send it as a
    // header with every fetch() so the server accepts our requests
    // (see AutoValidateAntiforgeryTokenAttribute in Program.cs).
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;

    // Same limits as on the server (Models/MediaImage.cs).
    const MAX_IMAGE_BYTES = 700 * 1024;
    const MAX_THUMB_BYTES = 200 * 1024;

    // The text area that images are inserted into: the last one you clicked
    // into (the About editor has two: English and Swedish).
    let activeTextarea = form.querySelector('.md-input');
    form.querySelectorAll('.md-input').forEach(t => t.addEventListener('focus', () => { activeTextarea = t; }));

    // ── Toolbar ──────────────────────────────────────────────

    // Replaces the selected text in a text area.
    // "transform" is a function that receives the selected text and returns
    // { text: the new text, select: [offset, length] of what to select after }.
    function replaceSelection(textarea, transform) {
        // "Destructuring": pull selectionStart (renamed start), selectionEnd
        // (renamed end) and value out of the textarea object in one line.
        const { selectionStart: start, selectionEnd: end, value } = textarea;
        const result = transform(value.slice(start, end));
        textarea.setRangeText(result.text, start, end, 'end');
        if (result.select) {
            // Select e.g. the placeholder "bold text" so you can type over it.
            const s = start + result.select[0];
            textarea.setSelectionRange(s, s + result.select[1]);
        }
        textarea.focus();
        // Tell the browser the content changed, like real typing would.
        textarea.dispatchEvent(new Event('input', { bubbles: true }));
    }

    // Puts "before" and "after" around the selection, e.g. ** ... ** for bold.
    // With nothing selected, inserts a placeholder such as "bold text".
    function wrap(textarea, before, after, placeholder) {
        replaceSelection(textarea, sel => {
            const inner = sel || placeholder;   // || = "or": use placeholder if sel is empty
            return { text: before + inner + after, select: [before.length, inner.length] };
        });
    }

    // Adds a prefix to every selected line, e.g. "- " for a bullet list.
    // It starts at the beginning of the first selected line.
    // prefixFor(i) returns the prefix for line number i (so numbered lists
    // can get "1. ", "2. ", ...).
    function prefixLines(textarea, prefixFor) {
        const lineStart = textarea.value.lastIndexOf('\n', textarea.selectionStart - 1) + 1;
        textarea.setSelectionRange(lineStart, textarea.selectionEnd);
        replaceSelection(textarea, sel => {
            const lines = (sel || 'Text').split('\n');
            return { text: lines.map((l, i) => prefixFor(i) + l).join('\n') };
        });
    }

    // What each toolbar button does. The keys match the buttons'
    // data-md="..." attributes in _MarkdownField.cshtml.
    const actions = {
        bold: t => wrap(t, '**', '**', 'bold text'),
        italic: t => wrap(t, '*', '*', 'italic text'),
        code: t => wrap(t, '`', '`', 'code'),
        heading: t => prefixLines(t, () => '## '),
        ul: t => prefixLines(t, () => '- '),
        ol: t => prefixLines(t, i => (i + 1) + '. '),
        quote: t => prefixLines(t, () => '> '),
        link: t => {
            // prompt() shows a small input box in the browser.
            const url = prompt('Link address (https://…)', 'https://');
            if (!url) return;
            replaceSelection(t, sel => {
                const text = sel || 'link text';
                return { text: '[' + text + '](' + url + ')', select: [1, text.length] };
            });
        },
    };

    // Connect each toolbar to its text area and preview box.
    form.querySelectorAll('.md-toolbar').forEach(toolbar => {
        // closest() walks up to the nearest parent matching the selector.
        const field = toolbar.closest('.md-field');
        const textarea = field.querySelector('.md-input');
        const preview = field.querySelector('.md-preview');

        // One click listener for the whole toolbar ("event delegation"):
        // we check which button inside it was clicked.
        toolbar.addEventListener('click', e => {
            const button = e.target.closest('button');
            if (!button) return;

            if (button.dataset.md) {
                // A formatting button: switch to Write and apply it.
                showTab(field, 'write');
                actions[button.dataset.md](textarea);
            } else if (button.dataset.mdTab) {
                // Write / Preview tab
                showTab(field, button.dataset.mdTab);
                if (button.dataset.mdTab === 'preview') loadPreview(textarea, preview);
            }
        });

        // Keyboard shortcuts: Ctrl+B, Ctrl+I, Ctrl+K (Cmd on a Mac).
        textarea.addEventListener('keydown', e => {
            if (!(e.ctrlKey || e.metaKey)) return;
            const key = { b: 'bold', i: 'italic', k: 'link' }[e.key.toLowerCase()];
            if (key) {
                e.preventDefault();   // stop the browser's own shortcut (e.g. Ctrl+B = bookmarks)
                actions[key](textarea);
            }
        });
    });

    // Shows either the text area ('write') or the preview ('preview').
    function showTab(field, tab) {
        field.querySelector('.md-input').hidden = tab !== 'write';
        field.querySelector('.md-preview').hidden = tab !== 'preview';
        field.querySelectorAll('.md-tab').forEach(b => {
            const active = b.dataset.mdTab === tab;
            b.classList.toggle('active', active);           // add/remove the CSS class
            b.setAttribute('aria-pressed', String(active));  // for screen readers
        });
    }

    // Sends the Markdown to the server (PostsController.Preview) and shows the
    // HTML it returns. The server does the converting, so the preview looks
    // exactly like the real page.
    async function loadPreview(textarea, preview) {
        preview.textContent = 'Loading preview…';
        try {
            // FormData = the same format a normal HTML form sends.
            const body = new FormData();
            body.append('markdown', textarea.value);
            const response = await fetch(form.dataset.previewUrl, {
                method: 'POST', body, headers: { RequestVerificationToken: token },
            });
            if (!response.ok) throw new Error();
            // The server returns sanitized HTML, so it is safe to insert
            preview.innerHTML = await response.text() || '<p class="form-hint">Nothing to preview.</p>';
        } catch {
            preview.textContent = 'Could not load the preview.';
        }
    }

    // ── Cover image ──────────────────────────────────────────
    // Only the post editor has a cover picker; on the About page these are null.
    const coverInput = document.getElementById('cover-image-id');
    const coverPreview = document.getElementById('cover-preview');
    const coverEmpty = form.querySelector('.cover-picker-empty');
    const coverRemove = document.getElementById('cover-remove');
    const allowCover = !!coverInput;   // "!!" turns a value into true/false

    // Sets (or with null: removes) the cover image and updates the preview.
    function setCover(id, thumbUrl) {
        if (!allowCover) return;
        coverInput.value = id || '';
        coverPreview.hidden = !id;
        coverRemove.hidden = !id;
        coverEmpty.hidden = !!id;
        if (id) coverPreview.src = thumbUrl;
        else coverPreview.removeAttribute('src');
    }

    // "?." = only call addEventListener if coverRemove exists.
    coverRemove?.addEventListener('click', () => setCover(null));

    // ── Image library ────────────────────────────────────────
    const grid = document.getElementById('media-grid');
    const status = document.getElementById('media-status');
    const empty = document.getElementById('media-empty');
    const fileInput = document.getElementById('media-file');
    const drop = document.getElementById('media-drop');

    // "my-summer_photo.jpg" -> "my summer photo" (used as the image description).
    // These are regular expressions: /\.[^.]+$/ = the file extension,
    // /[-_]+/g = all dashes and underscores.
    function altFromFileName(name) {
        return name.replace(/\.[^.]+$/, '').replace(/[-_]+/g, ' ').trim();
    }

    // Clicks on Insert / Cover / Delete buttons in the image grid.
    grid.addEventListener('click', async e => {
        const button = e.target.closest('button[data-media]');
        if (!button) return;
        const item = button.closest('.media-item');
        const { id, url, thumb } = item.dataset;

        if (button.dataset.media === 'insert') {
            // Insert Markdown image syntax at the cursor: ![description](/media/12)
            const alt = item.dataset.alt || 'Image';
            replaceSelection(activeTextarea, () => ({ text: '\n![' + alt + '](' + url + ')\n' }));
        } else if (button.dataset.media === 'cover') {
            setCover(id, thumb);
        } else if (button.dataset.media === 'delete') {
            if (!confirm('Delete this image? It disappears from every post that uses it.')) return;
            const body = new FormData();
            body.append('id', id);
            // Admin MediaController.Delete
            const response = await fetch(form.dataset.deleteUrl, {
                method: 'POST', body, headers: { RequestVerificationToken: token },
            });
            if (!response.ok) {
                status.textContent = 'Could not delete the image.';
                return;
            }
            if (allowCover && coverInput.value === id) setCover(null);
            item.remove();                                  // remove it from the page
            empty.hidden = grid.children.length > 0;
            status.textContent = 'Image deleted.';
        }
    });

    // Adds a newly uploaded image to the grid, building the same HTML as
    // _MediaLibrary.cshtml. createElement + textContent is used instead of
    // writing HTML strings, so a file name can never inject HTML.
    function addGridItem(image, fileName) {
        const li = document.createElement('li');
        li.className = 'media-item';
        li.dataset.id = image.id;
        li.dataset.url = image.url;
        li.dataset.thumb = image.thumbUrl;
        li.dataset.alt = image.alt;

        const img = document.createElement('img');
        img.src = image.thumbUrl;
        img.alt = image.alt;
        const name = document.createElement('span');
        name.className = 'media-name';
        name.textContent = fileName;
        name.title = fileName;
        const buttons = document.createElement('div');
        buttons.className = 'media-actions';
        // [action, label, extra CSS class]; skip "Cover" where there is no cover picker.
        const actionButtons = [['insert', 'Insert', ''], ['cover', 'Cover', ''], ['delete', '✕', ' btn-danger']]
            .filter(([action]) => action !== 'cover' || allowCover);
        for (const [action, label, cls] of actionButtons) {
            const b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-small' + cls;
            b.dataset.media = action;
            b.textContent = label;
            if (action === 'delete') b.setAttribute('aria-label', 'Delete image ' + fileName);
            buttons.appendChild(b);
        }
        li.append(img, name, buttons);
        grid.prepend(li);    // newest first
        empty.hidden = true;
    }

    // canvas.toBlob uses an old "callback" style; wrapping it in a Promise
    // lets us use "await" with it. A Blob is a chunk of binary data (the image file).
    function canvasToBlob(canvas, type, quality) {
        return new Promise(resolve => canvas.toBlob(resolve, type, quality));
    }

    // Draws the image at most maxSize px on the longest side and encodes it as
    // WebP (or JPEG where the browser cannot encode WebP) below maxBytes.
    // Strategy: try lower quality first; if still too big, shrink the image
    // by 20% and try again (up to 6 times).
    async function encode(bitmap, maxSize, maxBytes, startQuality) {
        let scale = Math.min(1, maxSize / Math.max(bitmap.width, bitmap.height));
        // A <canvas> is an in-memory drawing surface; it is never shown on the page.
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');

        for (let attempt = 0; attempt < 6; attempt++) {
            canvas.width = Math.max(1, Math.round(bitmap.width * scale));
            canvas.height = Math.max(1, Math.round(bitmap.height * scale));

            for (let quality = startQuality; quality >= 0.45; quality -= 0.1) {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                // Drawing the photo onto the canvas and saving it again creates
                // a brand-new file: hidden metadata (GPS etc.) is not copied.
                ctx.drawImage(bitmap, 0, 0, canvas.width, canvas.height);
                let blob = await canvasToBlob(canvas, 'image/webp', quality);

                // Some browsers (older Safari) can't create WebP and return PNG instead.
                if (!blob || blob.type !== 'image/webp') {
                    // JPEG has no transparency: paint a white background first
                    ctx.fillStyle = '#fff';
                    ctx.fillRect(0, 0, canvas.width, canvas.height);
                    ctx.drawImage(bitmap, 0, 0, canvas.width, canvas.height);
                    blob = await canvasToBlob(canvas, 'image/jpeg', quality);
                }
                if (blob && blob.size <= maxBytes) {
                    return { blob, width: canvas.width, height: canvas.height };
                }
            }
            scale *= 0.8;
        }
        // "throw" stops here and jumps to the nearest "catch" (in uploadAll).
        throw new Error('The image could not be made small enough.');
    }

    // Resizes one file and sends it to the server (Admin MediaController.Upload).
    async function upload(file) {
        let bitmap;
        try {
            // Decode the file into pixels. imageOrientation: 'from-image'
            // turns phone photos the right way up.
            bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' });
        } catch {
            throw new Error(file.name + ' could not be read as an image (try JPEG, PNG or WebP).');
        }

        const full = await encode(bitmap, 1600, MAX_IMAGE_BYTES, 0.82);   // the large version
        const thumb = await encode(bitmap, 800, MAX_THUMB_BYTES, 0.75);   // the thumbnail
        bitmap.close();   // free the memory

        const ext = full.blob.type === 'image/webp' ? '.webp' : '.jpg';
        const body = new FormData();
        body.append('image', full.blob, 'image' + ext);
        body.append('thumb', thumb.blob, 'thumb' + ext);
        body.append('fileName', file.name);
        body.append('altText', altFromFileName(file.name));
        body.append('width', full.width);
        body.append('height', full.height);

        const response = await fetch(form.dataset.uploadUrl, {
            method: 'POST', body, headers: { RequestVerificationToken: token },
        });
        // The server answers with JSON: { id, url, thumbUrl, alt } or { error }.
        const result = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(result.error || 'Upload failed for ' + file.name + '.');

        addGridItem(result, file.name);
    }

    // Uploads several files one after another, with progress in the status line.
    async function uploadAll(files) {
        // Array.from turns the FileList into a normal array; keep only images.
        const list = Array.from(files).filter(f => f.type.startsWith('image/'));
        let done = 0;
        for (const file of list) {
            status.textContent = 'Uploading ' + file.name + ' (' + (done + 1) + ' of ' + list.length + ')…';
            try {
                await upload(file);
                done++;
            } catch (err) {
                // Show the error and stop.
                status.textContent = err.message;
                return;
            }
        }
        status.textContent = done === 1 ? 'Image uploaded.' : done + ' images uploaded.';
    }

    // Files chosen with "Upload images"
    fileInput.addEventListener('change', () => {
        uploadAll(fileInput.files);
        fileInput.value = '';   // reset, so choosing the same file again still triggers "change"
    });

    // Drag and drop. preventDefault() stops the browser from simply opening
    // the dropped file; the 'dragging' class highlights the drop area.
    drop.addEventListener('dragover', e => {
        e.preventDefault();
        drop.classList.add('dragging');
    });
    drop.addEventListener('dragleave', () => drop.classList.remove('dragging'));
    drop.addEventListener('drop', e => {
        e.preventDefault();
        drop.classList.remove('dragging');
        uploadAll(e.dataTransfer.files);
    });
})();
