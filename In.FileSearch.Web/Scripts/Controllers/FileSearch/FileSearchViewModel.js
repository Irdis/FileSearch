(function () {
    function ExtensionViewModel(extensionInfo) {
        this.name = extensionInfo.Name;
        this.fileType = extensionInfo.FileExtension;
        this.options = extensionInfo.Options.map(function (o) {
            return {
                name: o.AttributeName,
                type: o.AttributeType,
                value: ko.observable(defaultVal(o.AttributeType))
            }
        });
    }

    function defaultVal(type) {
        switch (type) {
            case 0:
                return false;
            case 1:
                return 0;
            case 2:
                return '';
            default:
                return null;
        }
    }

    function FileSearchViewModel(extensions) {
        var that = this;
        this._streamer = $.connection.fileSearchHub;
        this._requestId = null;

        this.connected = ko.observable(false);
        this.path = ko.observable("c:\\");
        this.includeSubDir = ko.observable(false);
        this.createdFrom = ko.observable();
        this.createdTo = ko.observable();
        this.modifiedFrom = ko.observable();
        this.modifiedTo = ko.observable();
        this.readOnlyAttr = ko.observable(false);
        this.hiddenAttr = ko.observable(false);
        this.systemAttr = ko.observable(false);
        this.temporaryAttr = ko.observable(false);
        this.selectedExtension = ko.observable(null);
        this.extensions = extensions.map(function (e) { return new ExtensionViewModel(e) });

        this.results = ko.observableArray();
        this.hiddenResults = ko.observableArray();
        this.status = ko.observable(null);
        this.error = ko.observable();

        this.isRunning = ko.computed(function () {
            return that.status() === 'running' ||
                that.status() === 'cancelling' ||
                that.status() === 'stalled';
        });

        this.canRun = ko.computed(function () {
            return that.connected() && that.selectedExtension() != null &&
            (that.status() === 'completed' ||
                that.status() === 'canceled' ||
                that.status() === 'failed' ||
                that.status() === null
            );
        });
        this.canCancel = ko.computed(function () {
            return that.status() === 'running';
        });

        this._streamer.client.notify = function (msg) { onMessage(that, msg); }
        $.connection.hub.start().done(function () {
            that.connected(true);
        });
    }
    FileSearchViewModel.prototype.start = function () {
        var id = guid(),
            options = {};
        options.Directory = this.path();
        options.IncludeSubDir = this.includeSubDir();
        options.CreatedDateFrom = this.createdFrom() ? this.createdFrom() : null;
        options.CreatedDateTo = this.createdTo() ? this.createdTo() : null;
        options.ModifyDateFrom = this.modifiedFrom() ? this.modifiedFrom() : null;
        options.ModifyDateTo = this.modifiedTo() ? this.modifiedTo() : null;
        options.FileAttributes = fileAttributes(this);
        options.ExtensionName = this.selectedExtension().name;
        options.ExtensionOptions = this.selectedExtension().options.map(function (o) {
            return { Key: o.name, Value: o.value().toString() };
        });
        clear(this);
        this._requestId = id;
        sendRequest(this, id, options);
    }

    FileSearchViewModel.prototype.cancel = function () {
        cancelRequest(this, this._requestId);
    }

    FileSearchViewModel.prototype.loadMore = function () {
        for (var i = 0; i < 50; i++) {
            if (this.hiddenResults().length === 0) {
                return;
            }
            var item = this.hiddenResults.shift();
            this.results.push(item);
        }
    }

    function clear(that) {
        that.status(null);
        that.results([]);
        that.hiddenResults([]);
        that.error(null);
    }

    function cancelRequest(that, id) {
        that.status('cancelling');
        $.ajax({
            url: '/FileSearch/Stop',
            type: 'POST',
            data: JSON.stringify({
                id: id
            }),
            contentType: 'application/json; charset=utf-8',
            error: function () {
                that._streamer.server.unsubscribe(id);
                that.status('failed');
                that.error('Unexpected error');
            }
        });
    }
    function sendRequest(that, id, options) {
        that.status('stalled');
        that._streamer.server.subscribe(id).done(function () {
            $.ajax({
                url: '/FileSearch/Run',
                type: 'POST',
                data: JSON.stringify({
                    id: id,
                    options: options
                }),
                contentType: 'application/json; charset=utf-8',
                success: function () {
                    if (that.status() === 'stalled') {
                        that.status('running');
                    }
                },
                error: function () {
                    that._streamer.server.unsubscribe(id);
                    that.status('failed');
                    that.error('Unexpected error');
                }
            });
        });
    }

    function onMessage(that, msg) {
        if (msg.Completed) {
            if (msg.Canceled) {
                that.status('canceled');
            } else if (msg.Failed) {
                that.status('failed');
                that.error(msg.FailReason);
            } else {
                that.status('completed');
            }
            that._streamer.server.unsubscribe(that._requestId);
        } else {
            var item = {
                file: msg.FileName
            };
            if (that.results().length >= 50) {
                that.hiddenResults.push(item);
            } else {
                that.results.push(item);
            }
        }
    }

    function fileAttributes(that) {
        var result = 0;
        if (that.readOnlyAttr()) {
            result = result | 1;
        }
        if (that.hiddenAttr()) {
            result = result | 2;
        }
        if (that.systemAttr()) {
            result = result | 4;
        }
        if (that.temporaryAttr()) {
            result = result | 256;
        }
        return result === 0 ? null : result;
    }

    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
              .toString(16)
              .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
          s4() + '-' + s4() + s4() + s4();
    }
    window.FileSearchViewModel = FileSearchViewModel;

})();