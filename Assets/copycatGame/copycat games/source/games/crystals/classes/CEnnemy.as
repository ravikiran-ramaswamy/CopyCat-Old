class classes.CEnnemy
{
	var Clip:MovieClip;
	var Speed:Number;
	var sboom:Sound;
	
	public function CEnnemy(y:Number, s:Number, c:String){
		if(_root.move_counter <=110) {
			trace("new ennemy");
			this.Clip = _root.trans_game.background.attachMovie(c, "enemyship"+random(500),_root.trans_game.background.getNextHighestDepth());
			this.Clip._x = 800 + random(150);
			this.Clip._y = y;
			this.Clip.play();		
			this.Speed = s;
		}
	}
	public function Explose(){
		trace('Enemy down ! ' +this.Clip);		
		this.Clip.gotoAndPlay(2)
		delete this;
	}
	
	public function Move(x:Number, y:Number){
		this.Clip._x -= this.Speed;
		this.Clip.swapDepths(_root.trans_game.background.getNextHighestDepth());
	}
	
}