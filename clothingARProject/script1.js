console.log ("pot");
function add (val1, val2){
	return val1 + val2;
}

console.log (add(2, 5));

function toggle (num){
	const val = document.getElementById(num);
	console.log(val);
	console.log(val.style["font-size"]);
	if (val.style["font-size"] == "42px")
		val.style["font-size"] = "92px";
	else
		val.style["font-size"] = "42px";
}