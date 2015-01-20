var postsUri = '/api/Posts/';
var userPostsUri = '/api/Users/';
var postsMax = 20;

var PostsViewModel = function () {
	var self = this;
	self.error = ko.observable();
	self.posts = ko.observableArray();
	self.loading = ko.observable(false);
	self.hasMore = ko.observable(true);
	self.user = ko.observable();
	self.lastId = 0;
	self.getUri = function () {
		var uri = postsUri;
		if (self.user() != null && self.user().id() > 0) {
			uri = userPostsUri + self.user().id() + "/Follows/Posts/";
		}
		if (self.lastId > 0)
			uri += "?maxId=" + self.lastId;
		return uri;
	}

	self.getPosts = function () {
		if (self.hasMore()) {
			self.loading(true);
			ajaxHelper(self.getUri(), 'GET', self.error).done(function(data) {
				if (data.length < postsMax)
					self.hasMore(false);
				ko.utils.arrayForEach(data, function(post) {
					var res = new PostViewModel(post);
					self.lastId = !self.lastId || self.lastId > res.id() ? res.id() : self.lastId;
					self.posts.push(res);
				});
				self.loading(false);
			});
		}
	}

	self.initialLoad = function() {
		if (self.lastId === 0)
			self.getPosts();
	}
}

var ViewModel = function () {
	var self = this;
	var allUserVM = new PostsViewModel();
	self.error = ko.observable();
	self.allPosts = ko.observable(allUserVM);
	
	
	self.currentUser = ko.observable(new UserViewModel(getCurrentUser()));
	self.forUser = ko.observable(new PostsViewModel(""));
	self.forUser().user(self.currentUser());
	self.newItem = ko.observable(new PostViewModel(""));

	self.signUp = function() {
		ajaxHelper("/api/Users", 'POST', self.error, ({
				Name: self.currentUser().name()
		})).done(function (data) {
			var newUser = new UserViewModel(data);
			self.forUser().user(newUser);
				self.currentUser(newUser);
				saveCurrentUser(data);
			});
	}

	self.logIn = function() {
		ajaxHelper("/api/Users/" + self.currentUser().name(), 'GET', self.error).done(function (data) {
			var newUser = new UserViewModel(data);
			self.currentUser(newUser);
			self.forUser().user(newUser);
			saveCurrentUser(data);
		});
	};

	self.subscribe = function (post) {
		ajaxHelper("/api/Users/" + self.currentUser().id() + "/Followers", 'POST', self.error, {
			Id: post.author().id()
		}.done(function (data) {
			self.forUser(new PostsViewModel(self.currentUser()));
		}));
	}

	self.unsubscribe = function (post) {
		ajaxHelper("/api/Users/" + self.currentUser().id() + "/Followers/" + post.author().id(), 'DELETE', self.error).done(function (data) {
			self.forUser(new PostsViewModel(self.currentUser()));
		});
	}

	self.logOut = function() {
		self.currentUser(new UserViewModel(""));
		self.forUser(new PostsViewModel(""));
		$("#allItemsTab").tab('show');
		forgetUser();
	}

	self.addItem = function() {
		ajaxHelper("/api/Posts/", 'POST', self.error, {
			Text: self.newItem().text(),
			Author: {
				Id: self.currentUser().id()
	}
		}).done(function(data) {
			var post = new PostViewModel(data);
			self.newItem(new PostViewModel(""));
			self.allPosts().posts.push(post);
		});
	}

	allUserVM.getPosts();
}

ko.applyBindings(new ViewModel());

$('#tabs a').click(function (e) {
	e.preventDefault();
	$(this).tab('show');
})