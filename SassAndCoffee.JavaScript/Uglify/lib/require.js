(function () {
    var mappings = {};

    var require = function (path) {
        var result = mappings[path];
        return result;
    };

    require.publish = function (path, obj) {
        mappings[path] = obj;
    };

    this.require = require;
}).call(this);