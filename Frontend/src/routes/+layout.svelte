<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { onMount } from 'svelte';
    import {fetchChats} from '$lib/Handlers/groupHandler'
    import {getToken} from '$lib/Handlers/authHandler'
    


</script>
    

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="ie=edge">
</head>

<body>
    <nav class="navbar">
        <h1>Boomerangr</h1>
    </nav>  
    <slot></slot>
</body>




<style>   
h1{
    color: var(--prim-fg);
} 
     main{
        flex-grow:1;
        display: flex;
        flex-direction: row;
    }

    body{
        margin:0;
        width: 100wh;
        height: 100vh;
        display:flex;
        flex-direction: column;
    }
    .navbar{
        background-color: var(--prim);
        min-height: 10%;
        width: 100%; 
        z-index:100;    
        /*box-shadow 0 1px 2px 0 #color */  
        
    }
    .side-menu{
        background-color:var(--sec);
        min-width:100px;
        z-index: 50;

        /*box-shadow 1px 0 2px 0 #color */  
    }
    .room-button{
        color: white;
        background-color: darkgray;
        text-decoration: none;
        display: flex;
        justify-content: center;
        align-items: center;
        margin: 0.75rem 1rem;
        height: 50px;
        border-radius: 5px;
        box-shadow: 0 1px 3px 1px black;
        min-width: 6rem;
    }
    .room-button:hover{
        box-shadow: 0px 2px 3px 1px black;
    }

    .modal{
        display: flex;
        position: absolute;
        justify-content: center;
        align-items: center;
        flex-direction: column;
        min-height: 100vh;
        min-width: 100vw;
        z-index: 200;
        top:0;
        left:0;
        background-color: rgba(0.2, 0.2, 0.2, 0.8);
    }
  
    .modal-body > .close{
        position:absolute;
        top:5px;
        right: 5px;
        margin-top: 0;
    }
    .modal-body{
        position:relative;
        min-width:300px;
        display:flex;
        flex-direction: column;
        background-color: yellow;
        padding:2rem;
    }

    .modal-body > *{
        margin-top: 0.5rem;
    }
    .modal-body > header{
        color: #FFF;
        font-size:24px;
    }
    .modal-body>div>input{
        width:100%;
    }
    .modal-body > footer{
        text-align: center;
    }
</style>