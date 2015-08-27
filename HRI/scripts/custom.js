!function (a) {
    a(function () {
        a.support.transition = function () {
            var a = function () {
                var a = document.createElement("bootstrap"), b = { WebkitTransition: "webkitTransitionEnd", MozTransition: "transitionend", OTransition: "oTransitionEnd otransitionend", transition: "transitionend" }, c;
                for (c in b) if (a.style[c] !== undefined) return b[c]
            }();
            return a && { end: a }
        }()
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.options = c, this.$element = a(b).delegate('[data-dismiss="modal"]', "click.dismiss.modal", a.proxy(this.hide, this)), this.options.remote && this.$element.find(".modal-body").load(this.options.remote)
    };
    b.prototype = {
        constructor: b, toggle: function () {
            return this[this.isShown ? "hide" : "show"]()
        }, show: function () {
            var b = this, c = a.Event("show");
            this.$element.trigger(c);
            if (this.isShown || c.isDefaultPrevented()) return;
            this.isShown = !0, this.escape(), this.backdrop(function () {
                var c = a.support.transition && b.$element.hasClass("fade");
                b.$element.parent().length || b.$element.appendTo(document.body), b.$element.show(), c && b.$element[0].offsetWidth, b.$element.addClass("in").attr("aria-hidden", !1), b.enforceFocus(), c ? b.$element.one(a.support.transition.end, function () {
                    b.$element.focus().trigger("shown")
                }) : b.$element.focus().trigger("shown")
            })
        }, hide: function (b) {
            b && b.preventDefault();
            var c = this;
            b = a.Event("hide"), this.$element.trigger(b);
            if (!this.isShown || b.isDefaultPrevented()) return;
            this.isShown = !1, this.escape(), a(document).off("focusin.modal"), this.$element.removeClass("in").attr("aria-hidden", !0), a.support.transition && this.$element.hasClass("fade") ? this.hideWithTransition() : this.hideModal()
        }, enforceFocus: function () {
            var b = this;
            a(document).on("focusin.modal", function (a) {
                b.$element[0] !== a.target && !b.$element.has(a.target).length && b.$element.focus()
            })
        }, escape: function () {
            var a = this;
            this.isShown && this.options.keyboard ? this.$element.on("keyup.dismiss.modal", function (b) {
                b.which == 27 && a.hide()
            }) : this.isShown || this.$element.off("keyup.dismiss.modal")
        }, hideWithTransition: function () {
            var b = this, c = setTimeout(function () {
                b.$element.off(a.support.transition.end), b.hideModal()
            }, 500);
            this.$element.one(a.support.transition.end, function () {
                clearTimeout(c), b.hideModal()
            })
        }, hideModal: function () {
            var a = this;
            this.$element.hide(), this.backdrop(function () {
                a.removeBackdrop(), a.$element.trigger("hidden")
            })
        }, removeBackdrop: function () {
            this.$backdrop && this.$backdrop.remove(), this.$backdrop = null
        }, backdrop: function (b) {
            var c = this, d = this.$element.hasClass("fade") ? "fade" : "";
            if (this.isShown && this.options.backdrop) {
                var e = a.support.transition && d;
                this.$backdrop = a('<div class="modal-backdrop ' + d + '" />').appendTo(document.body), this.$backdrop.click(this.options.backdrop == "static" ? a.proxy(this.$element[0].focus, this.$element[0]) : a.proxy(this.hide, this)), e && this.$backdrop[0].offsetWidth, this.$backdrop.addClass("in");
                if (!b) return;
                e ? this.$backdrop.one(a.support.transition.end, b) : b()
            } else !this.isShown && this.$backdrop ? (this.$backdrop.removeClass("in"), a.support.transition && this.$element.hasClass("fade") ? this.$backdrop.one(a.support.transition.end, b) : b()) : b && b()
        }
    };
    var c = a.fn.modal;
    a.fn.modal = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("modal"), f = a.extend({}, a.fn.modal.defaults, d.data(), typeof c == "object" && c);
            e || d.data("modal", e = new b(this, f)), typeof c == "string" ? e[c]() : f.show && e.show()
        })
    }, a.fn.modal.defaults = { backdrop: !0, keyboard: !0, show: !0 }, a.fn.modal.Constructor = b, a.fn.modal.noConflict = function () {
        return a.fn.modal = c, this
    }, a(document).on("click.modal.data-api", '[data-toggle="modal"]', function (b) {
        var c = a(this), d = c.attr("href"), e = a(c.attr("data-target") || d && d.replace(/.*(?=#[^\s]+$)/, "")), f = e.data("modal") ? "toggle" : a.extend({ remote: !/#/.test(d) && d }, e.data(), c.data());
        b.preventDefault(), e.modal(f).one("hide", function () {
            c.focus()
        })
    })
}(window.jQuery), !function (a) {
    function b() {
        a(".dropdown-backdrop").remove(), a(d).each(function () {
            c(a(this)).removeClass("open")
        })
    }

    function c(b) {
        var c = b.attr("data-target"), d;
        c || (c = b.attr("href"), c = c && /#/.test(c) && c.replace(/.*(?=#[^\s]*$)/, "")), d = c && a(c);
        if (!d || !d.length) d = b.parent();
        return d
    }

    var d = "[data-toggle=dropdown]", e = function (b) {
        var c = a(b).on("click.dropdown.data-api", this.toggle);
        a("html").on("click.dropdown.data-api", function () {
            c.parent().removeClass("open")
        })
    };
    e.prototype = {
        constructor: e, toggle: function (d) {
            var e = a(this), f, g;
            if (e.is(".disabled, :disabled")) return;
            return f = c(e), g = f.hasClass("open"), b(), g || ("ontouchstart" in document.documentElement && a('<div class="dropdown-backdrop"/>').insertBefore(a(this)).on("click", b), f.toggleClass("open")), e.focus(), !1
        }, keydown: function (b) {
            var e, f, g, h, i, j;
            if (!/(38|40|27)/.test(b.keyCode)) return;
            e = a(this), b.preventDefault(), b.stopPropagation();
            if (e.is(".disabled, :disabled")) return;
            h = c(e), i = h.hasClass("open");
            if (!i || i && b.keyCode == 27) return b.which == 27 && h.find(d).focus(), e.click();
            f = a("[role=menu] li:not(.divider):visible a", h);
            if (!f.length) return;
            j = f.index(f.filter(":focus")), b.keyCode == 38 && j > 0 && j--, b.keyCode == 40 && j < f.length - 1 && j++, ~j || (j = 0), f.eq(j).focus()
        }
    };
    var f = a.fn.dropdown;
    a.fn.dropdown = function (b) {
        return this.each(function () {
            var c = a(this), d = c.data("dropdown");
            d || c.data("dropdown", d = new e(this)), typeof b == "string" && d[b].call(c)
        })
    }, a.fn.dropdown.Constructor = e, a.fn.dropdown.noConflict = function () {
        return a.fn.dropdown = f, this
    }, a(document).on("click.dropdown.data-api", b).on("click.dropdown.data-api", ".dropdown form", function (a) {
        a.stopPropagation()
    }).on("click.dropdown.data-api", d, e.prototype.toggle).on("keydown.dropdown.data-api", d + ", [role=menu]", e.prototype.keydown)
}(window.jQuery), !function (a) {
    function b(b, c) {
        var d = a.proxy(this.process, this), e = a(b).is("body") ? a(window) : a(b), f;
        this.options = a.extend({}, a.fn.scrollspy.defaults, c), this.$scrollElement = e.on("scroll.scroll-spy.data-api", d), this.selector = (this.options.target || (f = a(b).attr("href")) && f.replace(/.*(?=#[^\s]+$)/, "") || "") + " .nav li > a", this.$body = a("body"), this.refresh(), this.process()
    }

    b.prototype = {
        constructor: b, refresh: function () {
            var b = this, c;
            this.offsets = a([]), this.targets = a([]), c = this.$body.find(this.selector).map(function () {
                var c = a(this), d = c.data("target") || c.attr("href"), e = /^#\w/.test(d) && a(d);
                return e && e.length && [
                    [e.position().top + (!a.isWindow(b.$scrollElement.get(0)) && b.$scrollElement.scrollTop()), d]
                ] || null
            }).sort(function (a, b) {
                return a[0] - b[0]
            }).each(function () {
                b.offsets.push(this[0]), b.targets.push(this[1])
            })
        }, process: function () {
            var a = this.$scrollElement.scrollTop() + this.options.offset, b = this.$scrollElement[0].scrollHeight || this.$body[0].scrollHeight, c = b - this.$scrollElement.height(), d = this.offsets, e = this.targets, f = this.activeTarget, g;
            if (a >= c) return f != (g = e.last()[0]) && this.activate(g);
            for (g = d.length; g--;) f != e[g] && a >= d[g] && (!d[g + 1] || a <= d[g + 1]) && this.activate(e[g])
        }, activate: function (b) {
            var c, d;
            this.activeTarget = b, a(this.selector).parent(".active").removeClass("active"), d = this.selector + '[data-target="' + b + '"],' + this.selector + '[href="' + b + '"]', c = a(d).parent("li").addClass("active"), c.parent(".dropdown-menu").length && (c = c.closest("li.dropdown").addClass("active")), c.trigger("activate")
        }
    };
    var c = a.fn.scrollspy;
    a.fn.scrollspy = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("scrollspy"), f = typeof c == "object" && c;
            e || d.data("scrollspy", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.scrollspy.Constructor = b, a.fn.scrollspy.defaults = { offset: 10 }, a.fn.scrollspy.noConflict = function () {
        return a.fn.scrollspy = c, this
    }, a(window).on("load", function () {
        a('[data-spy="scroll"]').each(function () {
            var b = a(this);
            b.scrollspy(b.data())
        })
    })
}(window.jQuery), !function (a) {
    var b = function (b) {
        this.element = a(b)
    };
    b.prototype = {
        constructor: b, show: function () {
            var b = this.element, c = b.closest("ul:not(.dropdown-menu)"), d = b.attr("data-target"), e, f, g;
            d || (d = b.attr("href"), d = d && d.replace(/.*(?=#[^\s]*$)/, ""));
            if (b.parent("li").hasClass("active")) return;
            e = c.find(".active:last a")[0], g = a.Event("show", { relatedTarget: e }), b.trigger(g);
            if (g.isDefaultPrevented()) return;
            f = a(d), this.activate(b.parent("li"), c), this.activate(f, f.parent(), function () {
                b.trigger({ type: "shown", relatedTarget: e })
            })
        }, activate: function (b, c, d) {
            function e() {
                f.removeClass("active").find("> .dropdown-menu > .active").removeClass("active"), b.addClass("active"), g ? (b[0].offsetWidth, b.addClass("in")) : b.removeClass("fade"), b.parent(".dropdown-menu") && b.closest("li.dropdown").addClass("active"), d && d()
            }

            var f = c.find("> .active"), g = d && a.support.transition && f.hasClass("fade");
            g ? f.one(a.support.transition.end, e) : e(), f.removeClass("in")
        }
    };
    var c = a.fn.tab;
    a.fn.tab = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("tab");
            e || d.data("tab", e = new b(this)), typeof c == "string" && e[c]()
        })
    }, a.fn.tab.Constructor = b, a.fn.tab.noConflict = function () {
        return a.fn.tab = c, this
    }, a(document).on("click.tab.data-api", '[data-toggle="tab"], [data-toggle="pill"]', function (b) {
        b.preventDefault(), a(this).tab("show")
    })
}(window.jQuery), !function (a) {
    var b = function (a, b) {
        this.init("tooltip", a, b)
    };
    b.prototype = {
        constructor: b, init: function (b, c, d) {
            var e, f, g, h, i;
            this.type = b, this.$element = a(c), this.options = this.getOptions(d), this.enabled = !0, g = this.options.trigger.split(" ");
            for (i = g.length; i--;) h = g[i], h == "click" ? this.$element.on("click." + this.type, this.options.selector, a.proxy(this.toggle, this)) : h != "manual" && (e = h == "hover" ? "mouseenter" : "focus", f = h == "hover" ? "mouseleave" : "blur", this.$element.on(e + "." + this.type, this.options.selector, a.proxy(this.enter, this)), this.$element.on(f + "." + this.type, this.options.selector, a.proxy(this.leave, this)));
            this.options.selector ? this._options = a.extend({}, this.options, { trigger: "manual", selector: "" }) : this.fixTitle()
        }, getOptions: function (b) {
            return b = a.extend({}, a.fn[this.type].defaults, this.$element.data(), b), b.delay && typeof b.delay == "number" && (b.delay = { show: b.delay, hide: b.delay }), b
        }, enter: function (b) {
            var c = a.fn[this.type].defaults, d = {}, e;
            this._options && a.each(this._options, function (a, b) {
                c[a] != b && (d[a] = b)
            }, this), e = a(b.currentTarget)[this.type](d).data(this.type);
            if (!e.options.delay || !e.options.delay.show) return e.show();
            clearTimeout(this.timeout), e.hoverState = "in", this.timeout = setTimeout(function () {
                e.hoverState == "in" && e.show()
            }, e.options.delay.show)
        }, leave: function (b) {
            var c = a(b.currentTarget)[this.type](this._options).data(this.type);
            this.timeout && clearTimeout(this.timeout);
            if (!c.options.delay || !c.options.delay.hide) return c.hide();
            c.hoverState = "out", this.timeout = setTimeout(function () {
                c.hoverState == "out" && c.hide()
            }, c.options.delay.hide)
        }, show: function () {
            var b, c, d, e, f, g, h = a.Event("show");
            if (this.hasContent() && this.enabled) {
                this.$element.trigger(h);
                if (h.isDefaultPrevented()) return;
                b = this.tip(), this.setContent(), this.options.animation && b.addClass("fade"), f = typeof this.options.placement == "function" ? this.options.placement.call(this, b[0], this.$element[0]) : this.options.placement, b.detach().css({ top: 0, left: 0, display: "block" }), this.options.container ? b.appendTo(this.options.container) : b.insertAfter(this.$element), c = this.getPosition(), d = b[0].offsetWidth, e = b[0].offsetHeight;
                switch (f) {
                    case "bottom":
                        g = { top: c.top + c.height, left: c.left + c.width / 2 - d / 2 };
                        break;
                    case "top":
                        g = { top: c.top - e, left: c.left + c.width / 2 - d / 2 };
                        break;
                    case "left":
                        g = { top: c.top + c.height / 2 - e / 2, left: c.left - d };
                        break;
                    case "right":
                        g = { top: c.top + c.height / 2 - e / 2, left: c.left + c.width }
                }
                this.applyPlacement(g, f), this.$element.trigger("shown")
            }
        }, applyPlacement: function (a, b) {
            var c = this.tip(), d = c[0].offsetWidth, e = c[0].offsetHeight, f, g, h, i;
            c.offset(a).addClass(b).addClass("in"), f = c[0].offsetWidth, g = c[0].offsetHeight, b == "top" && g != e && (a.top = a.top + e - g, i = !0), b == "bottom" || b == "top" ? (h = 0, a.left < 0 && (h = a.left * -2, a.left = 0, c.offset(a), f = c[0].offsetWidth, g = c[0].offsetHeight), this.replaceArrow(h - d + f, f, "left")) : this.replaceArrow(g - e, g, "top"), i && c.offset(a)
        }, replaceArrow: function (a, b, c) {
            this.arrow().css(c, a ? 50 * (1 - a / b) + "%" : "")
        }, setContent: function () {
            var a = this.tip(), b = this.getTitle();
            a.find(".tooltip-inner")[this.options.html ? "html" : "text"](b), a.removeClass("fade in top bottom left right")
        }, hide: function () {
            function b() {
                var b = setTimeout(function () {
                    d.off(a.support.transition.end).detach()
                }, 500);
                d.one(a.support.transition.end, function () {
                    clearTimeout(b), d.detach()
                })
            }

            var c = this, d = this.tip(), e = a.Event("hide");
            this.$element.trigger(e);
            if (e.isDefaultPrevented()) return;
            return d.removeClass("in"), a.support.transition && this.$tip.hasClass("fade") ? b() : d.detach(), this.$element.trigger("hidden"), this
        }, fixTitle: function () {
            var a = this.$element;
            (a.attr("title") || typeof a.attr("data-original-title") != "string") && a.attr("data-original-title", a.attr("title") || "").attr("title", "")
        }, hasContent: function () {
            return this.getTitle()
        }, getPosition: function () {
            var b = this.$element[0];
            return a.extend({}, typeof b.getBoundingClientRect == "function" ? b.getBoundingClientRect() : { width: b.offsetWidth, height: b.offsetHeight }, this.$element.offset())
        }, getTitle: function () {
            var a, b = this.$element, c = this.options;
            return a = b.attr("data-original-title") || (typeof c.title == "function" ? c.title.call(b[0]) : c.title), a
        }, tip: function () {
            return this.$tip = this.$tip || a(this.options.template)
        }, arrow: function () {
            return this.$arrow = this.$arrow || this.tip().find(".tooltip-arrow")
        }, validate: function () {
            this.$element[0].parentNode || (this.hide(), this.$element = null, this.options = null)
        }, enable: function () {
            this.enabled = !0
        }, disable: function () {
            this.enabled = !1
        }, toggleEnabled: function () {
            this.enabled = !this.enabled
        }, toggle: function (b) {
            var c = b ? a(b.currentTarget)[this.type](this._options).data(this.type) : this;
            c.tip().hasClass("in") ? c.hide() : c.show()
        }, destroy: function () {
            this.hide().$element.off("." + this.type).removeData(this.type)
        }
    };
    var c = a.fn.tooltip;
    a.fn.tooltip = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("tooltip"), f = typeof c == "object" && c;
            e || d.data("tooltip", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.tooltip.Constructor = b, a.fn.tooltip.defaults = { animation: !0, placement: "top", selector: !1, template: '<div class="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>', trigger: "hover focus", title: "", delay: 0, html: !1, container: !1 }, a.fn.tooltip.noConflict = function () {
        return a.fn.tooltip = c, this
    }
}(window.jQuery), !function (a) {
    var b = function (a, b) {
        this.init("popover", a, b)
    };
    b.prototype = a.extend({}, a.fn.tooltip.Constructor.prototype, {
        constructor: b, setContent: function () {
            var a = this.tip(), b = this.getTitle(), c = this.getContent();
            a.find(".popover-title")[this.options.html ? "html" : "text"](b), a.find(".popover-content")[this.options.html ? "html" : "text"](c), a.removeClass("fade top bottom left right in")
        }, hasContent: function () {
            return this.getTitle() || this.getContent()
        }, getContent: function () {
            var a, b = this.$element, c = this.options;
            return a = (typeof c.content == "function" ? c.content.call(b[0]) : c.content) || b.attr("data-content"), a
        }, tip: function () {
            return this.$tip || (this.$tip = a(this.options.template)), this.$tip
        }, destroy: function () {
            this.hide().$element.off("." + this.type).removeData(this.type)
        }
    });
    var c = a.fn.popover;
    a.fn.popover = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("popover"), f = typeof c == "object" && c;
            e || d.data("popover", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.popover.Constructor = b, a.fn.popover.defaults = a.extend({}, a.fn.tooltip.defaults, { placement: "right", trigger: "click", content: "", template: '<div class="popover"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content"></div></div>' }), a.fn.popover.noConflict = function () {
        return a.fn.popover = c, this
    }
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.options = a.extend({}, a.fn.affix.defaults, c), this.$window = a(window).on("scroll.affix.data-api", a.proxy(this.checkPosition, this)).on("click.affix.data-api", a.proxy(function () {
            setTimeout(a.proxy(this.checkPosition, this), 1)
        }, this)), this.$element = a(b), this.checkPosition()
    };
    b.prototype.checkPosition = function () {
        if (!this.$element.is(":visible")) return;
        var b = a(document).height(), c = this.$window.scrollTop(), d = this.$element.offset(), e = this.options.offset, f = e.bottom, g = e.top, h = "affix affix-top affix-bottom", i;
        typeof e != "object" && (f = g = e), typeof g == "function" && (g = e.top()), typeof f == "function" && (f = e.bottom()), i = this.unpin != null && c + this.unpin <= d.top ? !1 : f != null && d.top + this.$element.height() >= b - f ? "bottom" : g != null && c <= g ? "top" : !1;
        if (this.affixed === i) return;
        this.affixed = i, this.unpin = i == "bottom" ? d.top - c : null, this.$element.removeClass(h).addClass("affix" + (i ? "-" + i : ""))
    };
    var c = a.fn.affix;
    a.fn.affix = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("affix"), f = typeof c == "object" && c;
            e || d.data("affix", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.affix.Constructor = b, a.fn.affix.defaults = { offset: 0 }, a.fn.affix.noConflict = function () {
        return a.fn.affix = c, this
    }, a(window).on("load", function () {
        a('[data-spy="affix"]').each(function () {
            var b = a(this), c = b.data();
            c.offset = c.offset || {}, c.offsetBottom && (c.offset.bottom = c.offsetBottom), c.offsetTop && (c.offset.top = c.offsetTop), b.affix(c)
        })
    })
}(window.jQuery), !function (a) {
    var b = '[data-dismiss="alert"]', c = function (c) {
        a(c).on("click", b, this.close)
    };
    c.prototype.close = function (b) {
        function c() {
            f.trigger("closed").remove()
        }

        var d = a(this), e = d.attr("data-target"), f;
        e || (e = d.attr("href"), e = e && e.replace(/.*(?=#[^\s]*$)/, "")), f = a(e), b && b.preventDefault(), f.length || (f = d.hasClass("alert") ? d : d.parent()), f.trigger(b = a.Event("close"));
        if (b.isDefaultPrevented()) return;
        f.removeClass("in"), a.support.transition && f.hasClass("fade") ? f.on(a.support.transition.end, c) : c()
    };
    var d = a.fn.alert;
    a.fn.alert = function (b) {
        return this.each(function () {
            var d = a(this), e = d.data("alert");
            e || d.data("alert", e = new c(this)), typeof b == "string" && e[b].call(d)
        })
    }, a.fn.alert.Constructor = c, a.fn.alert.noConflict = function () {
        return a.fn.alert = d, this
    }, a(document).on("click.alert.data-api", b, c.prototype.close)
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.options = a.extend({}, a.fn.button.defaults, c)
    };
    b.prototype.setState = function (a) {
        var b = "disabled", c = this.$element, d = c.data(), e = c.is("input") ? "val" : "html";
        a += "Text", d.resetText || c.data("resetText", c[e]()), c[e](d[a] || this.options[a]), setTimeout(function () {
            a == "loadingText" ? c.addClass(b).attr(b, b) : c.removeClass(b).removeAttr(b)
        }, 0)
    }, b.prototype.toggle = function () {
        var a = this.$element.closest('[data-toggle="buttons-radio"]');
        a && a.find(".active").removeClass("active"), this.$element.toggleClass("active")
    };
    var c = a.fn.button;
    a.fn.button = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("button"), f = typeof c == "object" && c;
            e || d.data("button", e = new b(this, f)), c == "toggle" ? e.toggle() : c && e.setState(c)
        })
    }, a.fn.button.defaults = { loadingText: "loading..." }, a.fn.button.Constructor = b, a.fn.button.noConflict = function () {
        return a.fn.button = c, this
    }, a(document).on("click.button.data-api", "[data-toggle^=button]", function (b) {
        var c = a(b.target);
        c.hasClass("btn") || (c = c.closest(".btn")), c.button("toggle")
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.options = a.extend({}, a.fn.collapse.defaults, c), this.options.parent && (this.$parent = a(this.options.parent)), this.options.toggle && this.toggle()
    };
    b.prototype = {
        constructor: b, dimension: function () {
            var a = this.$element.hasClass("width");
            return a ? "width" : "height"
        }, show: function () {
            var b, c, d, e;
            if (this.transitioning || this.$element.hasClass("in")) return;
            b = this.dimension(), c = a.camelCase(["scroll", b].join("-")), d = this.$parent && this.$parent.find("> .accordion-group > .in");
            if (d && d.length) {
                e = d.data("collapse");
                if (e && e.transitioning) return;
                d.collapse("hide"), e || d.data("collapse", null)
            }
            this.$element[b](0), this.transition("addClass", a.Event("show"), "shown"), a.support.transition && this.$element[b](this.$element[0][c])
        }, hide: function () {
            var b;
            if (this.transitioning || !this.$element.hasClass("in")) return;
            b = this.dimension(), this.reset(this.$element[b]()), this.transition("removeClass", a.Event("hide"), "hidden"), this.$element[b](0)
        }, reset: function (a) {
            var b = this.dimension();
            return this.$element.removeClass("collapse")[b](a || "auto")[0].offsetWidth, this.$element[a !== null ? "addClass" : "removeClass"]("collapse"), this
        }, transition: function (b, c, d) {
            var e = this, f = function () {
                c.type == "show" && e.reset(), e.transitioning = 0, e.$element.trigger(d)
            };
            this.$element.trigger(c);
            if (c.isDefaultPrevented()) return;
            this.transitioning = 1, this.$element[b]("in"), a.support.transition && this.$element.hasClass("collapse") ? this.$element.one(a.support.transition.end, f) : f()
        }, toggle: function () {
            this[this.$element.hasClass("in") ? "hide" : "show"]()
        }
    };
    var c = a.fn.collapse;
    a.fn.collapse = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("collapse"), f = a.extend({}, a.fn.collapse.defaults, d.data(), typeof c == "object" && c);
            e || d.data("collapse", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.collapse.defaults = { toggle: !0 }, a.fn.collapse.Constructor = b, a.fn.collapse.noConflict = function () {
        return a.fn.collapse = c, this
    }, a(document).on("click.collapse.data-api", "[data-toggle=collapse]", function (b) {
        var c = a(this), d, e = c.attr("data-target") || b.preventDefault() || (d = c.attr("href")) && d.replace(/.*(?=#[^\s]+$)/, ""), f = a(e).data("collapse") ? "toggle" : c.data();
        c[a(e).hasClass("in") ? "addClass" : "removeClass"]("collapsed"), a(e).collapse(f)
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.$indicators = this.$element.find(".carousel-indicators"), this.options = c, this.options.pause == "hover" && this.$element.on("mouseenter", a.proxy(this.pause, this)).on("mouseleave", a.proxy(this.cycle, this))
    };
    b.prototype = {
        cycle: function (b) {
            return b || (this.paused = !1), this.interval && clearInterval(this.interval), this.options.interval && !this.paused && (this.interval = setInterval(a.proxy(this.next, this), this.options.interval)), this
        }, getActiveIndex: function () {
            return this.$active = this.$element.find(".item.active"), this.$items = this.$active.parent().children(), this.$items.index(this.$active)
        }, to: function (b) {
            var c = this.getActiveIndex(), d = this;
            if (b > this.$items.length - 1 || b < 0) return;
            return this.sliding ? this.$element.one("slid", function () {
                d.to(b)
            }) : c == b ? this.pause().cycle() : this.slide(b > c ? "next" : "prev", a(this.$items[b]))
        }, pause: function (b) {
            return b || (this.paused = !0), this.$element.find(".next, .prev").length && a.support.transition.end && (this.$element.trigger(a.support.transition.end), this.cycle(!0)), clearInterval(this.interval), this.interval = null, this
        }, next: function () {
            if (this.sliding) return;
            return this.slide("next")
        }, prev: function () {
            if (this.sliding) return;
            return this.slide("prev")
        }, slide: function (b, c) {
            var d = this.$element.find(".item.active"), e = c || d[b](), f = this.interval, g = b == "next" ? "left" : "right", h = b == "next" ? "first" : "last", i = this, j;
            this.sliding = !0, f && this.pause(), e = e.length ? e : this.$element.find(".item")[h](), j = a.Event("slide", { relatedTarget: e[0], direction: g });
            if (e.hasClass("active")) return;
            this.$indicators.length && (this.$indicators.find(".active").removeClass("active"), this.$element.one("slid", function () {
                var b = a(i.$indicators.children()[i.getActiveIndex()]);
                b && b.addClass("active")
            }));
            if (a.support.transition && this.$element.hasClass("slide")) {
                this.$element.trigger(j);
                if (j.isDefaultPrevented()) return;
                e.addClass(b), e[0].offsetWidth, d.addClass(g), e.addClass(g), this.$element.one(a.support.transition.end, function () {
                    e.removeClass([b, g].join(" ")).addClass("active"), d.removeClass(["active", g].join(" ")), i.sliding = !1, setTimeout(function () {
                        i.$element.trigger("slid")
                    }, 0)
                })
            } else {
                this.$element.trigger(j);
                if (j.isDefaultPrevented()) return;
                d.removeClass("active"), e.addClass("active"), this.sliding = !1, this.$element.trigger("slid")
            }
            return f && this.cycle(), this
        }
    };
    var c = a.fn.carousel;
    a.fn.carousel = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("carousel"), f = a.extend({}, a.fn.carousel.defaults, typeof c == "object" && c), g = typeof c == "string" ? c : f.slide;
            e || d.data("carousel", e = new b(this, f)), typeof c == "number" ? e.to(c) : g ? e[g]() : f.interval && e.pause().cycle()
        })
    }, a.fn.carousel.defaults = { interval: 5e3, pause: "hover" }, a.fn.carousel.Constructor = b, a.fn.carousel.noConflict = function () {
        return a.fn.carousel = c, this
    }, a(document).on("click.carousel.data-api", "[data-slide], [data-slide-to]", function (b) {
        var c = a(this), d, e = a(c.attr("data-target") || (d = c.attr("href")) && d.replace(/.*(?=#[^\s]+$)/, "")), f = a.extend({}, e.data(), c.data()), g;
        e.carousel(f), (g = c.attr("data-slide-to")) && e.data("carousel").pause().to(g).cycle(), b.preventDefault()
    })
}(window.jQuery), !function (a) {
    var b = function (b, c) {
        this.$element = a(b), this.options = a.extend({}, a.fn.typeahead.defaults, c), this.matcher = this.options.matcher || this.matcher, this.sorter = this.options.sorter || this.sorter, this.highlighter = this.options.highlighter || this.highlighter, this.updater = this.options.updater || this.updater, this.source = this.options.source, this.$menu = a(this.options.menu), this.shown = !1, this.listen()
    };
    b.prototype = {
        constructor: b, select: function () {
            var a = this.$menu.find(".active").attr("data-value");
            return this.$element.val(this.updater(a)).change(), this.hide()
        }, updater: function (a) {
            return a
        }, show: function () {
            var b = a.extend({}, this.$element.position(), { height: this.$element[0].offsetHeight });
            return this.$menu.insertAfter(this.$element).css({ top: b.top + b.height, left: b.left }).show(), this.shown = !0, this
        }, hide: function () {
            return this.$menu.hide(), this.shown = !1, this
        }, lookup: function (b) {
            var c;
            return this.query = this.$element.val(), !this.query || this.query.length < this.options.minLength ? this.shown ? this.hide() : this : (c = a.isFunction(this.source) ? this.source(this.query, a.proxy(this.process, this)) : this.source, c ? this.process(c) : this)
        }, process: function (b) {
            var c = this;
            return b = a.grep(b, function (a) {
                return c.matcher(a)
            }), b = this.sorter(b), b.length ? this.render(b.slice(0, this.options.items)).show() : this.shown ? this.hide() : this
        }, matcher: function (a) {
            return ~a.toLowerCase().indexOf(this.query.toLowerCase())
        }, sorter: function (a) {
            var b = [], c = [], d = [], e;
            while (e = a.shift()) e.toLowerCase().indexOf(this.query.toLowerCase()) ? ~e.indexOf(this.query) ? c.push(e) : d.push(e) : b.push(e);
            return b.concat(c, d)
        }, highlighter: function (a) {
            var b = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
            return a.replace(new RegExp("(" + b + ")", "ig"), function (a, b) {
                return "<strong>" + b + "</strong>"
            })
        }, render: function (b) {
            var c = this;
            return b = a(b).map(function (b, d) {
                return b = a(c.options.item).attr("data-value", d), b.find("a").html(c.highlighter(d)), b[0]
            }), b.first().addClass("active"), this.$menu.html(b), this
        }, next: function (b) {
            var c = this.$menu.find(".active").removeClass("active"), d = c.next();
            d.length || (d = a(this.$menu.find("li")[0])), d.addClass("active")
        }, prev: function (a) {
            var b = this.$menu.find(".active").removeClass("active"), c = b.prev();
            c.length || (c = this.$menu.find("li").last()), c.addClass("active")
        }, listen: function () {
            this.$element.on("focus", a.proxy(this.focus, this)).on("blur", a.proxy(this.blur, this)).on("keypress", a.proxy(this.keypress, this)).on("keyup", a.proxy(this.keyup, this)), this.eventSupported("keydown") && this.$element.on("keydown", a.proxy(this.keydown, this)), this.$menu.on("click", a.proxy(this.click, this)).on("mouseenter", "li", a.proxy(this.mouseenter, this)).on("mouseleave", "li", a.proxy(this.mouseleave, this))
        }, eventSupported: function (a) {
            var b = a in this.$element;
            return b || (this.$element.setAttribute(a, "return;"), b = typeof this.$element[a] == "function"), b
        }, move: function (a) {
            if (!this.shown) return;
            switch (a.keyCode) {
                case 9:
                case 13:
                case 27:
                    a.preventDefault();
                    break;
                case 38:
                    a.preventDefault(), this.prev();
                    break;
                case 40:
                    a.preventDefault(), this.next()
            }
            a.stopPropagation()
        }, keydown: function (b) {
            this.suppressKeyPressRepeat = ~a.inArray(b.keyCode, [40, 38, 9, 13, 27]), this.move(b)
        }, keypress: function (a) {
            if (this.suppressKeyPressRepeat) return;
            this.move(a)
        }, keyup: function (a) {
            switch (a.keyCode) {
                case 40:
                case 38:
                case 16:
                case 17:
                case 18:
                    break;
                case 9:
                case 13:
                    if (!this.shown) return;
                    this.select();
                    break;
                case 27:
                    if (!this.shown) return;
                    this.hide();
                    break;
                default:
                    this.lookup()
            }
            a.stopPropagation(), a.preventDefault()
        }, focus: function (a) {
            this.focused = !0
        }, blur: function (a) {
            this.focused = !1, !this.mousedover && this.shown && this.hide()
        }, click: function (a) {
            a.stopPropagation(), a.preventDefault(), this.select(), this.$element.focus()
        }, mouseenter: function (b) {
            this.mousedover = !0, this.$menu.find(".active").removeClass("active"), a(b.currentTarget).addClass("active")
        }, mouseleave: function (a) {
            this.mousedover = !1, !this.focused && this.shown && this.hide()
        }
    };
    var c = a.fn.typeahead;
    a.fn.typeahead = function (c) {
        return this.each(function () {
            var d = a(this), e = d.data("typeahead"), f = typeof c == "object" && c;
            e || d.data("typeahead", e = new b(this, f)), typeof c == "string" && e[c]()
        })
    }, a.fn.typeahead.defaults = { source: [], items: 8, menu: '<ul class="typeahead dropdown-menu"></ul>', item: '<li><a href="#"></a></li>', minLength: 1 }, a.fn.typeahead.Constructor = b, a.fn.typeahead.noConflict = function () {
        return a.fn.typeahead = c, this
    }, a(document).on("focus.typeahead.data-api", '[data-provide="typeahead"]', function (b) {
        var c = a(this);
        if (c.data("typeahead")) return;
        c.typeahead(c.data())
    })
}(window.jQuery);
(function () {
    $(document).ready(function () {
        var a = function (a) {
            a.preventDefault();
            var b = $(this), c = b.closest(".hri-accordion"), d = b.find(".hri-accordion-icon"), e = b.closest(".hri-accordion").find(".hri-accordion-content");
            e.stop().slideToggle(function () {
                c.toggleClass("hri-accordion-state-expanded");
                return true
            })
        };
        var b = function (a) {
            a.preventDefault();
            var b = $(this), c = b.hasClass("hri-accordion-toggle-all-state-expanded"), d = b.text(), e = b.data("accordion-toggle-all-text");
            if (c) {
                b.text(e);
                b.data("accordion-toggle-all-text", d);
                b.removeClass("hri-accordion-toggle-all-state-expanded");
                $(".hri-accordion").each(function () {
                    var a = $(this);
                    if (a.hasClass("hri-accordion-state-expanded")) {
                        a.find(".hri-accordion-header").click()
                    } else {
                    }
                })
            } else {
                b.text(e);
                b.data("accordion-toggle-all-text", d);
                b.addClass("hri-accordion-toggle-all-state-expanded");
                $(".hri-accordion").each(function () {
                    var a = $(this);
                    if (a.hasClass("hri-accordion-state-expanded")) {
                    } else {
                        a.find(".hri-accordion-header").click()
                    }
                })
            }
        };
        $(".hri-accordion").on("click.toggle-one.hri-accordion", "div.hri-accordion-header", a);
        $(".hri-accordion-toggle-all").on("click.toggle-all.hri-accordion", null, b)
    })
})();
(function () {
    $(document).ready(function() {
        $("#hri-carousel").carousel();
    });
})();
(function () {
    $(document).ready(function () {
        var a = true;
        var b = {};
        b.tenant = "national";
        var c = function () {
            if ($("body").hasClass("hri-newyork")) {
                b.tenant = "ny"
            } else if ($("body").hasClass("hri-newjersey")) {
                b.tenant = "nj"
            } else if ($("body").hasClass("hri-oregon")) {
                b.tenant = "or"
            }
        };
        c();
        $('.shop-block input[type="checkbox"]').each(function () {
            if ($(this).prop("checked")) {
                $(this).closest(".field-block").find(".hri-hide").removeClass("hri-hide")
            }
        });
        
        var f = function (a) {
            var b = a.split("-");
            return b
        };
        var g = function (a) {
            var b = f(hri_planEffectiveDate);
            var c = new Date(parseInt(b[0], 10), parseInt(b[1], 10) - 1, parseInt(b[2], 10));
            b = f(a);
            var a = new Date(parseInt(b[0], 10), parseInt(b[1], 10) - 1, parseInt(b[2], 10)), d = c.getFullYear() - a.getFullYear(), e = c.getMonth() - a.getMonth();
            if (e < 0 || e === 0 && c.getDate() < a.getDate()) {
                d = d - 1
            }
            return d
        };
        var h = function () {
            var b = true;
            $(".shop-block").each(function () {
                b = b && $(this).data("stepvalid")
            });
            if (b) {
                $("#shop-form .shop-submit").removeClass("disabled");
                a = true
            } else {
                $("#shop-form .shop-submit").addClass("disabled");
                a = false
            }
        };
        var i = function (a, b) {
            var c = a.find(".textify-block");
            $fieldBlock = a.find(".field-block");
            if (c.hasClass("hri-hide")) {
                c.find("div").html(b);
                c.removeClass("hri-hide");
                $fieldBlock.addClass("hri-hide")
            } else {
                c.addClass("hri-hide");
                $fieldBlock.removeClass("hri-hide")
            }
        };
    })
})();
(function (a) {
    a(function () {
        a(".scroller").click(function () {
            var b = a(this).prop("hash");
            var c = a(b);
            if (b && c.length) {
                a("html, body").animate({ scrollTop: c.offset().top }, "slow", function () {
                    document.location.hash = b
                });
                return false
            }
        });
        a(".expand-content").hide();
        a(".expand-all").click(function () {
            var b = a(this);
            var c = a(".expand-group");
            var d = c.children(".expand-content");
            var e = a(".expand-toggle");
            if (d.is(":hidden")) {
                e.text("[ - ]");
                c.addClass("active");
                d.slideDown("fast");
                b.text("[ - ] Collapse all")
            } else {
                c.removeClass("active");
                d.slideUp("fast");
                b.text("[ + ] Expand all");
                e.text("[ + ]")
            }
            return false
        });
        a(".expand-link").click(function () {
            var b = a(this);
            var c = b.parents(".expand-group");
            var d = c.children(".expand-content");
            var e = b.find(".expand-toggle");
            var f = a(".expand-all");
            if (d.is(":hidden")) {
                e.text("[ - ]");
                c.addClass("active");
                d.slideDown("fast")
            } else {
                e.text("[ + ]");
                c.removeClass("active");
                d.slideUp("fast");
                f.text("[ + ] Expand all")
            }
            return false
        })
    })
})(jQuery);
(function () {
    $(document).ready(function () {
        var a = {
            $flowbar: $(".flowbar"), flowmessage: false, flowseconds: 0, flowtimer: "", init: function () {
                a.flowmessage = a.$flowbar.find("div.container").html();
                a.hasMessage = /<[^<]+?>/g.test(a.flowmessage);
                a.flowseconds = parseInt(a.$flowbar.data("flowseconds")) || 0;
                if (a.hasMessage) {
                    a.$flowbar.slideDown(500);
                    if (a.flowseconds) {
                        a.flowtimer = setTimeout(function () {
                            a.$flowbar.slideUp(500)
                        }, a.flowseconds * 1e3)
                    }
                }
            }
        };
        a.init()
    })
})();
/**
 *  Ajax Autocomplete for jQuery, version 1.2.9
 *  (c) 2013 Tomas Kirda
 *
 *  Ajax Autocomplete for jQuery is freely distributable under the terms of an MIT-style license.
 *  For details, see the web site: https://github.com/devbridge/jQuery-Autocomplete
 *
 */
(function (d) { "function" === typeof define && define.amd ? define(["jquery"], d) : d(jQuery) })(function (d) {
    function g(a, b) {
        var c = function () { }, c = {
            autoSelectFirst: !1, appendTo: "body", serviceUrl: null, lookup: null, onSelect: null, width: "auto", minChars: 1, maxHeight: 300, deferRequestBy: 0, params: {}, formatResult: g.formatResult, delimiter: null, zIndex: 9999, type: "GET", noCache: !1, onSearchStart: c, onSearchComplete: c, onSearchError: c, containerClass: "autocomplete-suggestions", tabDisabled: !1, dataType: "text", currentRequest: null, triggerSelectOnValidInput: !0,
            lookupFilter: function (a, b, c) { return -1 !== a.value.toLowerCase().indexOf(c) }, paramName: "query", transformResult: function (a) { return "string" === typeof a ? d.parseJSON(a) : a }
        }; this.element = a; this.el = d(a); this.suggestions = []; this.badQueries = []; this.selectedIndex = -1; this.currentValue = this.element.value; this.intervalId = 0; this.cachedResponse = {}; this.onChange = this.onChangeInterval = null; this.isLocal = !1; this.suggestionsContainer = null; this.options = d.extend({}, c, b); this.classes = { selected: "autocomplete-selected", suggestion: "autocomplete-suggestion" };
        this.hint = null; this.hintValue = ""; this.selection = null; this.initialize(); this.setOptions(b)
    } var k = function () { return { escapeRegExChars: function (a) { return a.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&") }, createNode: function (a) { var b = document.createElement("div"); b.className = a; b.style.position = "absolute"; b.style.display = "none"; return b } } }(); g.utils = k; d.Autocomplete = g; g.formatResult = function (a, b) { var c = "(" + k.escapeRegExChars(b) + ")"; return a.value.replace(RegExp(c, "gi"), "<strong>$1</strong>") }; g.prototype =
  {
      killerFn: null, initialize: function () {
          var a = this, b = "." + a.classes.suggestion, c = a.classes.selected, e = a.options, f; a.element.setAttribute("autocomplete", "off"); a.killerFn = function (b) { 0 === d(b.target).closest("." + a.options.containerClass).length && (a.killSuggestions(), a.disableKillerFn()) }; a.suggestionsContainer = g.utils.createNode(e.containerClass); f = d(a.suggestionsContainer); f.appendTo(e.appendTo); "auto" !== e.width && f.width(e.width); f.on("mouseover.autocomplete", b, function () { a.activate(d(this).data("index")) });
          f.on("mouseout.autocomplete", function () { a.selectedIndex = -1; f.children("." + c).removeClass(c) }); f.on("click.autocomplete", b, function () { a.select(d(this).data("index")) }); a.fixPosition(); a.fixPositionCapture = function () { a.visible && a.fixPosition() }; d(window).on("resize.autocomplete", a.fixPositionCapture); a.el.on("keydown.autocomplete", function (b) { a.onKeyPress(b) }); a.el.on("keyup.autocomplete", function (b) { a.onKeyUp(b) }); a.el.on("blur.autocomplete", function () { a.onBlur() }); a.el.on("focus.autocomplete", function () { a.onFocus() });
          a.el.on("change.autocomplete", function (b) { a.onKeyUp(b) })
      }, onFocus: function () { this.fixPosition(); if (this.options.minChars <= this.el.val().length) this.onValueChange() }, onBlur: function () { this.enableKillerFn() }, setOptions: function (a) { var b = this.options; d.extend(b, a); if (this.isLocal = d.isArray(b.lookup)) b.lookup = this.verifySuggestionsFormat(b.lookup); d(this.suggestionsContainer).css({ "max-height": b.maxHeight + "px", width: b.width + "px", "z-index": b.zIndex }) }, clearCache: function () {
          this.cachedResponse = {}; this.badQueries =
          []
      }, clear: function () { this.clearCache(); this.currentValue = ""; this.suggestions = [] }, disable: function () { this.disabled = !0; this.currentRequest && this.currentRequest.abort() }, enable: function () { this.disabled = !1 }, fixPosition: function () { var a; "body" === this.options.appendTo && (a = this.el.offset(), a = { top: a.top + this.el.outerHeight() + "px", left: a.left + "px" }, "auto" === this.options.width && (a.width = this.el.outerWidth() - 2 + "px"), d(this.suggestionsContainer).css(a)) }, enableKillerFn: function () {
          d(document).on("click.autocomplete",
          this.killerFn)
      }, disableKillerFn: function () { d(document).off("click.autocomplete", this.killerFn) }, killSuggestions: function () { var a = this; a.stopKillSuggestions(); a.intervalId = window.setInterval(function () { a.hide(); a.stopKillSuggestions() }, 50) }, stopKillSuggestions: function () { window.clearInterval(this.intervalId) }, isCursorAtEnd: function () {
          var a = this.el.val().length, b = this.element.selectionStart; return "number" === typeof b ? b === a : document.selection ? (b = document.selection.createRange(), b.moveStart("character",
          -a), a === b.text.length) : !0
      }, onKeyPress: function (a) {
          if (!this.disabled && !this.visible && 40 === a.which && this.currentValue) this.suggest(); else if (!this.disabled && this.visible) {
              switch (a.which) {
                  case 27: this.el.val(this.currentValue); this.hide(); break; case 39: if (this.hint && this.options.onHint && this.isCursorAtEnd()) { this.selectHint(); break } return; case 9: if (this.hint && this.options.onHint) { this.selectHint(); return } case 13: if (-1 === this.selectedIndex) { this.hide(); return } this.select(this.selectedIndex); if (9 === a.which &&
                  !1 === this.options.tabDisabled) return; break; case 38: this.moveUp(); break; case 40: this.moveDown(); break; default: return
              } a.stopImmediatePropagation(); a.preventDefault()
          }
      }, onKeyUp: function (a) { var b = this; if (!b.disabled) { switch (a.which) { case 38: case 40: return } clearInterval(b.onChangeInterval); if (b.currentValue !== b.el.val()) if (b.findBestHint(), 0 < b.options.deferRequestBy) b.onChangeInterval = setInterval(function () { b.onValueChange() }, b.options.deferRequestBy); else b.onValueChange() } }, onValueChange: function () {
          var a =
          this.options, b = this.el.val(), c = this.getQuery(b); this.selection && (this.selection = null, (a.onInvalidateSelection || d.noop).call(this.element)); clearInterval(this.onChangeInterval); this.currentValue = b; this.selectedIndex = -1; if (a.triggerSelectOnValidInput && (b = this.findSuggestionIndex(c), -1 !== b)) { this.select(b); return } c.length < a.minChars ? this.hide() : this.getSuggestions(c)
      }, findSuggestionIndex: function (a) {
          var b = -1, c = a.toLowerCase(); d.each(this.suggestions, function (a, d) {
              if (d.value.toLowerCase() === c) return b =
              a, !1
          }); return b
      }, getQuery: function (a) { var b = this.options.delimiter; if (!b) return a; a = a.split(b); return d.trim(a[a.length - 1]) }, getSuggestionsLocal: function (a) { var b = this.options, c = a.toLowerCase(), e = b.lookupFilter, f = parseInt(b.lookupLimit, 10), b = { suggestions: d.grep(b.lookup, function (b) { return e(b, a, c) }) }; f && b.suggestions.length > f && (b.suggestions = b.suggestions.slice(0, f)); return b }, getSuggestions: function (a) {
          var b, c = this, e = c.options, f = e.serviceUrl, l, g; e.params[e.paramName] = a; l = e.ignoreParams ? null : e.params;
          c.isLocal ? b = c.getSuggestionsLocal(a) : (d.isFunction(f) && (f = f.call(c.element, a)), g = f + "?" + d.param(l || {}), b = c.cachedResponse[g]); b && d.isArray(b.suggestions) ? (c.suggestions = b.suggestions, c.suggest()) : c.isBadQuery(a) || !1 === e.onSearchStart.call(c.element, e.params) || (c.currentRequest && c.currentRequest.abort(), c.currentRequest = d.ajax({ url: f, data: l, type: e.type, dataType: e.dataType }).done(function (b) { c.currentRequest = null; c.processResponse(b, a, g); e.onSearchComplete.call(c.element, a) }).fail(function (b, d, f) {
              e.onSearchError.call(c.element,
                a, b, d, f)
          }))
      }, isBadQuery: function (a) { for (var b = this.badQueries, c = b.length; c--;) if (0 === a.indexOf(b[c])) return !0; return !1 }, hide: function () { this.visible = !1; this.selectedIndex = -1; d(this.suggestionsContainer).hide(); this.signalHint(null) }, suggest: function () {
          if (0 === this.suggestions.length) this.hide(); else {
              var a = this.options, b = a.formatResult, c = this.getQuery(this.currentValue), e = this.classes.suggestion, f = this.classes.selected, g = d(this.suggestionsContainer), k = a.beforeRender, m = "", h; if (a.triggerSelectOnValidInput &&
            (h = this.findSuggestionIndex(c), -1 !== h)) { this.select(h); return } d.each(this.suggestions, function (a, d) { m += '<div class="' + e + '" data-index="' + a + '">' + b(d, c) + "</div>" }); "auto" === a.width && (h = this.el.outerWidth() - 2, g.width(0 < h ? h : 300)); g.html(m); a.autoSelectFirst && (this.selectedIndex = 0, g.children().first().addClass(f)); d.isFunction(k) && k.call(this.element, g); g.show(); this.visible = !0; this.findBestHint()
          }
      }, findBestHint: function () {
          var a = this.el.val().toLowerCase(), b = null; a && (d.each(this.suggestions, function (c,
                                                                                        d) { var f = 0 === d.value.toLowerCase().indexOf(a); f && (b = d); return !f }), this.signalHint(b))
      }, signalHint: function (a) { var b = ""; a && (b = this.currentValue + a.value.substr(this.currentValue.length)); this.hintValue !== b && (this.hintValue = b, this.hint = a, (this.options.onHint || d.noop)(b)) }, verifySuggestionsFormat: function (a) { return a.length && "string" === typeof a[0] ? d.map(a, function (a) { return { value: a, data: null } }) : a }, processResponse: function (a, b, c) {
          var d = this.options; a = d.transformResult(a, b); a.suggestions = this.verifySuggestionsFormat(a.suggestions);
          d.noCache || (this.cachedResponse[c] = a, 0 === a.suggestions.length && this.badQueries.push(c)); b === this.getQuery(this.currentValue) && (this.suggestions = a.suggestions, this.suggest())
      }, activate: function (a) { var b = this.classes.selected, c = d(this.suggestionsContainer), e = c.children(); c.children("." + b).removeClass(b); this.selectedIndex = a; return -1 !== this.selectedIndex && e.length > this.selectedIndex ? (a = e.get(this.selectedIndex), d(a).addClass(b), a) : null }, selectHint: function () {
          var a = d.inArray(this.hint, this.suggestions);
          this.select(a)
      }, select: function (a) { this.hide(); this.onSelect(a) }, moveUp: function () { -1 !== this.selectedIndex && (0 === this.selectedIndex ? (d(this.suggestionsContainer).children().first().removeClass(this.classes.selected), this.selectedIndex = -1, this.el.val(this.currentValue), this.findBestHint()) : this.adjustScroll(this.selectedIndex - 1)) }, moveDown: function () { this.selectedIndex !== this.suggestions.length - 1 && this.adjustScroll(this.selectedIndex + 1) }, adjustScroll: function (a) {
          var b = this.activate(a), c, e; b && (b = b.offsetTop,
          c = d(this.suggestionsContainer).scrollTop(), e = c + this.options.maxHeight - 25, b < c ? d(this.suggestionsContainer).scrollTop(b) : b > e && d(this.suggestionsContainer).scrollTop(b - this.options.maxHeight + 25), this.el.val(this.getValue(this.suggestions[a].value)), this.signalHint(null))
      }, onSelect: function (a) {
          var b = this.options.onSelect; a = this.suggestions[a]; this.currentValue = this.getValue(a.value); this.el.val(this.currentValue); this.signalHint(null); this.suggestions = []; this.selection = a; d.isFunction(b) && b.call(this.element,
          a)
      }, getValue: function (a) { var b = this.options.delimiter, c; if (!b) return a; c = this.currentValue; b = c.split(b); return 1 === b.length ? a : c.substr(0, c.length - b[b.length - 1].length) + a }, dispose: function () { this.el.off(".autocomplete").removeData("autocomplete"); this.disableKillerFn(); d(window).off("resize.autocomplete", this.fixPositionCapture); d(this.suggestionsContainer).remove() }
  }; d.fn.autocomplete = function (a, b) {
      return 0 === arguments.length ? this.first().data("autocomplete") : this.each(function () {
          var c = d(this), e =
          c.data("autocomplete"); if ("string" === typeof a) { if (e && "function" === typeof e[a]) e[a](b) } else e && e.dispose && e.dispose(), e = new g(this, a), c.data("autocomplete", e)
      })
  }
});

(function () {
    $(document).ready(function () {

        $('#shop-zipcode').autocomplete({
            paramName: 'zipCode',
            lookupLimit: 3,
            minChars: 2,
            serviceUrl: '/Umbraco/Surface/ComparePlansSurface/GetZipCodeList',
            onSelect: function (suggestion) {
                $('#shop-county').text(suggestion.data).show();
                $('#shop-zipcode').valid();
            },
            transformResult: function (response) {
                response = JSON.parse(response);
                return {
                    suggestions: $.map(response, function (dataItem) {
                        return { value: dataItem.zipCode, data: dataItem.county };
                    })
                };
            }
        });

        var initVisibilityFunc = function (id) {
            return function () {
                if (this.checked) {
                    $(id).show();
                } else {
                    $(id).hide();
                }
            };
        }

        $('#shop-coverself-check').click(function () {
            $("#shop-coverself").toggle(this.checked);
        }).each(initVisibilityFunc("#shop-coverself"));

        $('#shop-coverpartner-check').click(function () {
            $("#shop-coverpartner").toggle(this.checked);
        }).each(initVisibilityFunc("#shop-coverpartner"));

        var addChild = function () {
            var template = childTemplate.clone();
            template.find('input').attr('name', 'ChildrenAges');
            template.find('.age').text(childAmmount);
            attachHandlers(template);
            
            template.appendTo('#shop-coverchild');
            $('input[name="ChildrenAges"]').inputmask('Regex', { regex: '^[0-9]{1,3}(\.[0-9])?$' });
            childAmmount++;
        };

        var removeChild = function (element) {
            $(element).parents('.child').remove();
            if ($('#shop-coverchild').children().length == 1) {
                $("#shop-coverchild").toggle(false);
                $('#shop-coverchildren-check').prop('checked', false);
            }
            childAmmount--;

            $.each($('#shop-coverchild .child'), function (index, child) {
                $(child).find('.age').text(index);
            });
        };

        var attachHandlers = function (parent) {
            parent = $(parent);

            parent.find('.add-child').click(function (e) {
                e.preventDefault();
                addChild();
            });

            parent.find('.remove-child').click(function (e) {
                e.preventDefault();
                removeChild(this);
            });
        };

        var childTemplate = $('#shop-coverchild .template').clone().removeClass('template'),
          childAmmount = 1;
        $('#shop-coverchildren-check').click(function () {
            $("#shop-coverchild").toggle(this.checked);
            if (this.checked) {
                if ($('#shop-coverchild').children().length == 1)
                    addChild();
            }
        }).each(initVisibilityFunc("#shop-coverchild"));

        $('#shop-coverchild').find('.child').not('.template').each(function () {
            ++childAmmount;
            attachHandlers(this);
        });

        $('.hri-registration input[name=user_type]').change(function () {
            var type = $(this).val();
            if (type == 'register')
                $('#register-form').show();
            else
                $('#register-form').hide();
            $('.hri-registration .selection .desc').hide();
            $(this).next().next().css('display', 'inline-block');
        });
    })

    // Disable buttons after click
    $('form').submit(function (e) {
        if ($(this).valid && $(this).valid() || !$(this).valid) {
            $(this).find('.disable-after-click').prop('disabled', true).addClass('disabled');
            $(this).find('.submit-idle-helper').show();
        }
    });
})();