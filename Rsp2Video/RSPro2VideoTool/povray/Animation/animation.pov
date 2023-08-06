// Animation gif for RSPro2Video

#version 3.7;

global_settings {
  assumed_gamma 2.2
  }

 #include "colors.inc"
 #include "textures.inc"
 #include "shapes.inc"   

/*
// Overview camera.
camera {
    location <0, 200, -1>
    angle 30 //   direction <0, 0, 2>
    right x*image_width/image_height // keep propotions with any aspect ratio
    look_at <-10, 2.2, 0>
}
*/


// View camera.
camera {
    location <-11.5, 5, -12.5>
    angle 40 //   direction <0, 0, 2>
    right x*image_width/image_height // keep propotions with any aspect ratio
    look_at <-10, 2.17, 0>
}


light_source { 
    < 0, 200, -1> 
    colour White 
    spotlight 
    radius 8 
    falloff 50
    tightness 10
    point_at <-10, 2.2, 0>
}

// Sky.
sphere { <0, 0, 0>, 1
    hollow on
    texture { 
        pigment { gradient y 
            color_map {
                [0.0, 0.5 color White  color Gray30]
                [0.5, 1.0 color Gray30 color White]
            } 
        }
        finish {ambient 1 diffuse 0} 
        scale <2, 2, 2>
    }
    scale 100000
    // translate <0,-7000, 0>
}

#declare fractalColor1 = <0.5, 1.0, 0.0>;
#declare fractalColor2 = <0.0, 1.0, 0.0>;
#declare fractalColor3 = <0.0, 1.0, 0.5>;
#declare fractalColor4 = <0.0, 0.5, 1.0>;
#declare fractalColor5 = <0.0, 0.0, 1.0>;
#declare fractalColor6 = <0.5, 0.0, 1.0>;
#declare fractalColor7 = <1.0, 0.0, 0.5>;
#declare fractalColor8 = <1.0, 0.0, 0.0>;
#declare fractalColor9 = <1.0, 0.5, 0.0>;
#declare fractalColor0 = <1.0, 1.0, 1.0>;  
#declare fractalColorA = <0.5, 0.5, 0.5>;

// Ground plane.
plane { 
    <0, 1, 0>, 1 
    texture { 
        pigment { 
            mandel 1024                  
            color_map {
                [ 0.00 rgb <1, 1, 1>]
                [ 0.05 rgb <0, 0, 0>]
                [ 0.10 rgb <1, 1, 1>]
                [ 0.15 rgb <0, 0, 0>]
                [ 0.20 rgb <1, 1, 1>]
                [ 0.25 rgb <0, 0, 0>]
                [ 0.30 rgb <1, 1, 1>]
                [ 0.35 rgb <0, 0, 0>]
                [ 0.40 rgb <1, 1, 1>]
                [ 0.45 rgb <0, 0, 0>]
                [ 0.50 rgb <1, 1, 1>]
                [ 0.55 rgb <0, 0, 0>]
                [ 0.60 rgb <1, 1, 1>]
                [ 0.65 rgb <0, 0, 0>]
                [ 0.70 rgb <1, 1, 1>]
                [ 0.75 rgb <0, 0, 0>]
                [ 0.80 rgb <1, 1, 1>]
                [ 0.85 rgb <0, 0, 0>]
                [ 0.90 rgb <1, 1, 1>]
                [ 0.95 rgb <0, 0, 0>]
                [ 1.00 rgb <1, 1, 1>]
            }

            rotate <90, 0, 0>
            translate <1.24599859, 0, 0.092505>
            scale 10000000
        } 
    }
}  

/*
// target sphere
sphere { <0, 0, 0>, 0.5
            texture { pigment { Red } }
            translate <0, 2, 0>
        }
*/

#declare totalSmallObj = 512;

#macro MakeSmallobjs(objNum)       
        
    #local inverseClock = -clock;
    #local zRot1 = (360*8)/totalSmallObj * (objNum + (inverseClock * 2));
    #local zRot2 = (360*12*2*1)/totalSmallObj * (objNum + (inverseClock * 2));
    #local yRot2 = 360/totalSmallObj * (objNum + (inverseClock * 2));

    object {
        #local objSize = 0.05;
        
        box { <-objSize, -objSize, -objSize>, <objSize, objSize, objSize> texture { pigment { White } } }
        
        translate <0.55, 0, 0> rotate <0, 0, zRot2>
        translate <0, 1, 0>   rotate <0, 0, zRot1>
        translate <12, 0, 0>   rotate <0, -yRot2, 0>      
    } 

#end

#declare totalLargeOjb = 48;

#macro Makelargeojbs(objNum)       
    
    #local zRot1 = (360*8)/totalLargeOjb * (objNum + clock);
    #local yRot2 = 360/totalLargeOjb * (objNum + clock);

    object {
        #local objSize = 0.4;
        
        sphere { <0, 0, 0>, objSize
            texture { pigment { Black } finish { Mirror } }
            translate <0, 0, 0>
        }

        translate <0, 1, 0>   rotate <0, 0, zRot1>
        translate <12, 0, 0>   rotate <0, -yRot2, 0>  
        // normal { waves 0.35 frequency 4 scale 1 phase clock }    
    } 

#end        

// Create objects in scene.  

#declare spinner =
union {

    #for (smallOjbIndex, 0, totalSmallObj - 1)
        MakeSmallobjs(smallOjbIndex)
    #end
    
    #for (largeOjbIndex, 0, totalLargeOjb - 1)
        Makelargeojbs(largeOjbIndex)
    #end
}


union { 
    object { spinner rotate <0, 3, 0> translate <-19, 0, 0> } 
    //object { spinner rotate <0, 0, 180>  rotate <0, 0, 0> }

    translate <0, 2.7, 0>    
    rotate <0, 11.25, 0>
}
