class classes.CAmmo
{
	var Clip:MovieClip;
	var Speed:Number;	
			
	public function CAmmo(x:Number, y:Number, s:Number, c:String, id:Number){
		this.Speed = s;
		this.Clip = _root.trans_game.background.attachMovie(c, "ammo"+id, _root.trans_game.background.getNextHighestDepth());
		this.Clip._x = x;
		this.Clip._y = y;		 
	}
	public function Move(){
		this.Clip._x += this.Speed;
	}
	public function Remove(){
		trace("deleting ammo : "+this.Clip);
		this.Clip.removeMovieClip();
		delete this;
	}
	
}