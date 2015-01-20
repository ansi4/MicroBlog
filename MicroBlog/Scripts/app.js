var usersUri = function(userId) {
	return '/api/Users/' + (!userId ? "" : userId);
}

var userApiUri = function(userId) {
	var prefix = "/api/User/";
	this.posts = prefix + userId + "/Posts/";
	this.followers = prefix + userId + "/Followers/";
	this.follows = prefix + userId + "/Follows/";
	return this;
}


var PostViewToModel = function (post) {
	var self = this;
	self.text = ko.observable(post.text || "");
	self.internal = function () {
		return {
			Text: self.text()
		}
	};
}

var getCurrentUser = function() {
	var stored = localStorage['user'];
	var user;
	if (stored) {
		user = JSON.parse(stored);
	} else
		user = "";
	return user;
}

var saveCurrentUser = function(user) {
	localStorage['user'] = JSON.stringify(user);
}

var forgetUser = function() {
	localStorage.removeItem('user');
}

var postSort = function (l, r) {
	return r.id() - l.id();
}

var PostViewModel = function (post) {
	this.id = ko.observable(post.Id || "");
	this.text = ko.observable(post.Text || "");
	this.time = ko.observable(post.CreationTime ? new Date(post.CreationTime) : "");
	this.author = ko.observable(new UserViewModel(post.Author || ""));
}

var UserViewModel = function(user) {
	var self = this;
	self.id = ko.observable(user.Id || "");
	self.name = ko.observable(user.Name || "");
	self.following = ko.observableArray();
	if (user.following) {
		ko.helper.forEach(user.Follows, function (followed) {
			self.following.push(new UserViewModel(followed))
		});
	};
}

var UserViewToModel = function (user) {
	var self = this;
	self.id = ko.observable(user.id || "");
	self.name = ko.observable(user.name || "");

	self.internal = function () {
		return {
			Id: self.id() || 0,
			Name: self.name()
		}
	}
}

ajaxHelper = function ajaxHelper(uri, method, error, data) {
	error("");
	return $.ajax({
		type: method,
		url: uri,
		dataType: 'json',
		contentType: 'application/json',
		data: data ? JSON.stringify(data) : null
	}).fail(function (jqXHR, textStatus, errorThrown) {
		if (jqXHR.responseJSON)
			if (jqXHR.responseJSON.ExceptionMessage)
				error(jqXHR.responseJSON.ExceptionMessage);
			else
				error(jqXHR.responseJSON.Message);
		else
			error(errorThrown);
	});
}

