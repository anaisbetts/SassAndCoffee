var uglifyjs = require("uglify-js");

//function compilify_ujs(code, options) {
//    if (typeof (options) === "string") {
//        options = eval(options);
//    }
//    options = options || {
//        strict_semicolons: undefined,
//        mangle_options: {
//            toplevel: false,
//            except: null,
//            defines: {},
//        },
//        squeeze_options: {
//            make_seqs: true,
//            dead_code: true,
//        },
//        gen_options: {
//            beautify: false,
//            indent_start: 0,
//            indent_level: 4,
//            quote_keys: false,
//            space_colon: false,
//            ascii_only: false,
//            inline_script: false,
//        },
//    };
function compilify_ujs(code) {
    return uglifyjs(code);
};