// global variables
const vars = {
    CUSTOM_UPLOUD_PRESET: "custom_upload_preset",
    CLOUDINARY_ENV: "ulingrms",
    CLOUDINARY_BASE_URL:
        "https://api.cloudinary.com/v1_1/ulingrms/image/upload", // Upload REST API Template: "api.cloudinary.com/v1_1/<cloud name>/<resource_type>/upload"
};

// for uploading images in cloudinary (START)

async function postFetchUpload(url = "", data) {
    // Default options are marked with *
    const response = await fetch(url, {
        method: "POST", // *GET, POST, PUT, DELETE, etc.
        mode: "cors", // no-cors, *cors, same-origin
        cache: "no-cache", // *default, no-cache, reload, force-cache, only-if-cached
        credentials: "same-origin", // include, *same-origin, omit
        redirect: "follow", // manual, *follow, error
        referrerPolicy: "no-referrer", // no-referrer, *no-referrer-when-downgrade, origin, origin-when-cross-origin, same-origin, strict-origin, strict-origin-when-cross-origin, unsafe-url
        body: data, // body data type must match "Content-Type" header
    });
    return response.json(); // parses JSON response into native JavaScript objects
}

// for uploading images in cloudinary (END)

// validator function (start)
function isValid(caller) {
    if (caller.val() == "") {
        caller.addClass("is-invalid");
        return false;
    } else {
        caller.removeClass("is-invalid");
        return true;
    }
}

function isImgValid(caller, preview) {
    if (caller.val().toString() == "") {
        preview.addClass("img-invalid");
        return false;
    } else {
        preview.removeClass("img-invalid");
        return true;
    }
}

// validator function (end)

// this function if for button loading functionality
// disabling the button and replace the text with "Loading...." text,
// (START)

function btnLoading(isLoading, btn, normalText) {
    if (isLoading) {
        btn.addClass("disabled");
        btn.text("Loading...");
    } else {
        btn.removeClass("disabled");
        btn.text(normalText);
    }
}

// (END)
