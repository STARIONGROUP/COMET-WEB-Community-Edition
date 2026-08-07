    # Keyboard navigation and shortcuts

The COMET WEB application can be operated without a mouse. This page lists the keyboard controls that are available.

The custom shortcuts use **Alt+Shift+&lt;key&gt;**, chosen to avoid clashing with browser menu access keys. On Windows, `Alt+Shift` on its own can also switch the keyboard layout; pressing it together with the letter performs the application shortcut.

## Global shortcuts

These work from anywhere in the application.

| Shortcut | Action |
|----------|--------|
| `Alt` + `Shift` + `N` | Move focus to the **navigation** side bar |
| `Alt` + `Shift` + `T` | Move focus to the open **tabs** strip (the current tab, or the first one) |
| `Alt` + `Shift` + `M` | Move focus to the **main content** |
| `Alt` + `Shift` + `B` | Fold / unfold (collapse / expand) the side **bar** |

## Skip links

When a page first loads, pressing `Tab` reveals two "skip links" at the top-left:

- **Skip to navigation** &rarr; jumps to the side bar.
- **Skip to main content** &rarr; jumps to the page content.

Press `Enter` on a skip link to move there. These need no special key combination and work even when scripting is unavailable.

## Moving within the side bar

While focus is inside the side bar:

| Key | Action |
|-----|--------|
| `Arrow Down` / `Arrow Up` | Move to the next / previous entry |
| `Home` / `End` | Move to the first / last entry |
| `Enter` / `Space` | Open the focused application |

The currently open application is announced to assistive technologies with `aria-current="page"`.

## Moving within the tabs strip

While focus is inside the open-tabs strip:

| Key | Action |
|-----|--------|
| `Arrow Right` / `Arrow Left` | Move to the next / previous tab |
| `Home` / `End` | Move to the first / last tab |
| `Enter` / `Space` | Switch to the focused tab |
| `Tab` | Reach a tab's copy / close buttons |

## General controls

| Key | Action |
|-----|--------|
| `Tab` / `Shift` + `Tab` | Move focus forward / backward through every interactive element |
| `Enter` / `Space` | Activate the focused button or entry |
| `Arrow` keys | Move within a grid, combo box or menu (provided by the DevExpress component library) |
| `Esc` | Close an open drop-down, pop-up or dialog |

A visible focus outline is shown on every element that receives keyboard focus, so you can always see where you are.

## Rebinding the shortcuts

The `Alt+Shift` letters are defined in a single `shortcutKeys` map at the top of
[`COMETwebapp/wwwroot/Scripts/cometKeyboard.js`](../COMETwebapp/wwwroot/Scripts/cometKeyboard.js);
change them there if a combination clashes with your browser or operating system.
