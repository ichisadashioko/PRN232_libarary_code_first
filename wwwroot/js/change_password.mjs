import { get_current_token, invalidate_token, set_token } from "/js/utils.mjs";

document.addEventListener("DOMContentLoaded", function () {
    const token = get_current_token();
    if (token) {
        // Fetch user info using token
        fetch("/api/users/info", {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }).then(res => {
            if (!res.ok) {
                console.error("Failed to fetch user info:", res.statusText);
                console.error("Response status:", res.status);
                console.error("Response headers:", res.headers);

                window.location.href = `/login?return_url=${encodeURIComponent(window.location.pathname + window.location.search)}`;
                // throw new Error("Invalid token");
            }
            return res.json();
        }).then(user => {
            set_token(token); // Ensure token is set in localStorage and cookies
            // window.location.href = "/profile";
            const username = user.username;
            let button = document.getElementById("change-password-button");
            if (button != null) {
                try {
                    button.addEventListener('click', function (e) {
                        e.preventDefault();
                        const old_password = document.getElementById("current-password").value;
                        const new_password = document.getElementById("new-password").value;
                        const confirm_password = document.getElementById("confirm-password").value;
                        if (new_password !== confirm_password) {
                            alert("New password and confirmation do not match.");
                            return;
                        }

                        fetch("/api/users/changepassword", {
                            method: "POST",
                            headers: {
                                'Authorization': `Bearer ${token}`,
                                "Content-Type": "application/json",
                            },
                            body: JSON.stringify({
                                username,
                                old_password,
                                new_password
                            })
                        }).then(res => {
                            if (!res.ok) {
                                console.error("Failed to change password:", res.statusText);
                                console.error("Response status:", res.status);
                                console.error("Response headers:", res.headers);
                                throw new Error("Failed to change password");
                            }
                            return res.json();
                        }).then(data => {
                            alert("Password changed successfully");
                            // Optionally redirect or clear form fields
                            document.getElementById("current-password").value = '';
                            document.getElementById("new-password").value = '';
                            document.getElementById("confirm-password").value = '';
                        }).catch(err => {
                            console.error(err);
                            alert("Error changing password: " + err.message);
                        });
                    });
                } catch (error) {
                    console.error("Error adding event listener to change password button:", error);
                    alert("An error occurred while trying to change the password. Please try again later.");
                }
            }
        }).catch(() => {
            console.error(arguments);
            console.error("Token is invalid or expired");
            // Token invalid, show login form
            invalidate_token();
            window.location.href = `/login?return_url=${encodeURIComponent(window.location.pathname + window.location.search)}`;
        });
    } else {
        // No token, redirect to login
        window.location.href = `/login?return_url=${encodeURIComponent(window.location.pathname + window.location.search)}`;
    }
});
