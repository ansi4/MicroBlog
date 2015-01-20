

var ListViewModel = function (uri, itemToViewModel, viewModelToItem, connectedListViewSelector) {
	var self = this;
	self.uri = uri;
	self.items = ko.observableArray();
	self.itemsLoaded = ko.observable(false);
	self.ViewConstructor = itemToViewModel;
	self.ModelConstructor = viewModelToItem;
	self.connectedList = connectedListViewSelector;
	self.newItem = ko.observable(new self.ModelConstructor(""));
	self.error = ko.observable("");
	self.showItems = function () {
		if (!self.itemsLoaded()) {
			ajaxHelper(self.uri, 'GET', self.error).done(function (result) {
				ko.utils.arrayForEach(result, function (item) {
					self.items.push(new self.ViewConstructor(item));
				});
				self.itemsLoaded(true);
			});
		}
	}
	self.addItem = function () {
		ajaxHelper(self.uri, 'POST', self.error, self.newItem().internal()).done(function (result) {
			self.items.push(new self.ViewConstructor(result));
			self.newItem(new self.ModelConstructor(""));
			if (self.connectedList) {
				var lst = self.connectedList(result.Id);
				if (lst && lst.view().itemsLoaded())
					lst.view().items.push(lst.current);
			};
		});
	}

	self.removeItem = function (item) {
		ajaxHelper(self.uri + item.id(), 'DELETE', self.error).done(function (result) {
			self.items.remove(item);
			if (self.connecedList) {
				var lst = self.connecedList(result.Id);
				if (lst) {
					var connected = ko.utils.arrayFirst(lst.view().items(), function (obj) {
						return obj.id() === lst.current.id();
					});
					lst.view().items.remove(connected);
				}
			};
		});
	}
}

ko.bindingHandlers.clearNewUser = {
	update: function (element, valueAccessor) {
		if (valueAccessor() !== "" && element.value !== "") {
			var ph = element.placeholder;
			var newItem = ko.dataFor(element).newItem;
			if (valueAccessor() === newItem().id())
				newItem().name("");
			else
				newItem().id("");
			element.placeholder = ph;
		}
	}
}

var UserViewModel = function (user) {
	var self = this;
	self.id = ko.observable(user.Id || "");
	self.name = ko.observable(user.Name || "");
	self.isSelected = ko.observable(false);
	self.active = ko.pureComputed(function () {
		return self.isSelected() ? "active" : "";
	});

	self.postsView = ko.observable(new ListViewModel(userApiUri(user.Id).posts, PostViewModel, PostViewToModel));
	self.followersView = ko.observable(new ListViewModel(userApiUri(user.Id).followers, UserViewModel, UserViewToModel, function (id) {
		var followerUser = self.findUser(id);
		if (followerUser)
			return {
				view: followerUser.followedView,
				current: self
			}
		return null;
	}));

	self.followedView = ko.observable(new ListViewModel(userApiUri(user.Id).follows, UserViewModel, UserViewToModel, function (id) {
		var followedUser = self.findUser(id);
		if (followedUser)
			return {
				view: followedUser.followersView,
				current: self
			}
		return null;
	}));
}



var userSort = function (l, r) {
	var res = l.name().localeCompare(r.name());
	return res !== 0 ? res : l.id() - r.id();
}

var ViewModel = function () {
	var self = this;
	self.error = ko.observable();

	self.users = ko.observableArray();
	self.selectedUser = ko.observable();

	self.creatingUser = ko.observable(false);
	self.newUser = ko.observable();
	self.loading = ko.observable(false);

	self.addUser = function () {
		var user = {
			Name: self.newUser().name()
		}
		self.newUser("");
		ajaxHelper(usersUri(), 'POST', self.error, user).done(function (usr) {
			var res = new UserViewModel(usr);

			self.users.push(res);
			res.selectUser();
		});
	}

	self.createUser = function () {
		if (self.selectedUser())
			self.selectedUser().isSelected(false);
		self.newUser({
			name: ko.observable()
		});
		self.creatingUser(true);
		self.selectedUser(null);
	}

	UserViewModel.prototype.selectUser = function () {
		if (self.selectedUser())
			self.selectedUser().isSelected(false);
		this.isSelected(true);
		self.selectedUser(this);
		self.creatingUser(false);
	}

	UserViewModel.prototype.deleteUser = function () {
		var user = this;
		ajaxHelper(usersUri(user.id()), 'DELETE', self.error).done(function () {
			self.users.remove(user);
		});
	}

	UserViewModel.prototype.findUser = function (id) {
		return ko.utils.arrayFirst(self.users(), function (item) {
			return item.id() === id;
		});
	}

	function getAllUsers() {
		self.loading(true);
		ajaxHelper(usersUri(), 'GET', self.error).done(function (data) {
			ko.utils.arrayForEach(data, function (user) {
				var res = new UserViewModel(user);
				self.users.push(res);
			});
			self.loading(false);
		});
	}

	getAllUsers();
};

ko.applyBindings(document.vm = new ViewModel());